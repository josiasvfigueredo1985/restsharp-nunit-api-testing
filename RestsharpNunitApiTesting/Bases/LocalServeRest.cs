using RestSharpNunitApiTesting.RestSharpNunitApiTesting.Helpers;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RestSharpNunitApiTesting
{
    internal static class LocalServeRest
    {
        private const string Image = "paulogoncalvesbh/serverest:2.29.7";
        private static string containerId;

        public static void StartIfNeeded()
        {
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("URL")))
            {
                return;
            }

            containerId = RunDocker("run", "--detach", "--rm", "--publish", "127.0.0.1::3000", Image);
            try
            {
                string binding = RunDocker("port", containerId, "3000/tcp");
                const string prefix = "127.0.0.1:";
                if (!binding.StartsWith(prefix, StringComparison.Ordinal) ||
                    !int.TryParse(binding.Substring(prefix.Length), out int port) ||
                    port < 1 || port > 65535)
                {
                    throw new InvalidOperationException($"Unexpected local ServeRest port binding: {binding}");
                }

                string url = $"http://127.0.0.1:{port}/";
                WaitUntilReady(url);
                JsonBuilder.UpdateParameterAppSettings("URL", url);
                JsonBuilder.UpdateParameterAppSettings("NAME", "Local Test Administrator");
                JsonBuilder.UpdateParameterAppSettings("AUTHENTICATOR_USER", $"local-admin-{Guid.NewGuid():N}@example.com");
                JsonBuilder.UpdateParameterAppSettings("AUTHENTICATOR_PASSWORD", Guid.NewGuid().ToString("N"));
            }
            catch
            {
                Stop();
                throw;
            }
        }

        public static void Stop()
        {
            if (containerId == null)
            {
                return;
            }

            string id = containerId;
            containerId = null;
            RunDocker("stop", id);
        }

        private static void WaitUntilReady(string url)
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            Exception lastError = null;
            for (int attempt = 0; attempt < 60; attempt++)
            {
                try
                {
                    using HttpResponseMessage response = client.GetAsync(url + "usuarios").GetAwaiter().GetResult();
                    response.EnsureSuccessStatusCode();
                    return;
                }
                catch (HttpRequestException error)
                {
                    lastError = error;
                }
                catch (TaskCanceledException error)
                {
                    lastError = error;
                }

                Thread.Sleep(500);
            }

            throw new InvalidOperationException("The local ServeRest container did not become ready.", lastError);
        }

        private static string RunDocker(params string[] arguments)
        {
            var startInfo = new ProcessStartInfo("docker")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            foreach (string argument in arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }

            using Process process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Could not start Docker.");
            Task<string> output = process.StandardOutput.ReadToEndAsync();
            Task<string> error = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(120000))
            {
                process.Kill(true);
                process.WaitForExit();
                throw new TimeoutException("Docker did not respond within two minutes.");
            }

            string stdout = output.GetAwaiter().GetResult().Trim();
            string stderr = error.GetAwaiter().GetResult().Trim();
            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"Docker failed (exit code {process.ExitCode}): {stderr}");
            }

            return stdout;
        }
    }
}
