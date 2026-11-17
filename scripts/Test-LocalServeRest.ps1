param(
    [string]$Filter
)

$ErrorActionPreference = "Stop"
$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$image = "paulogoncalvesbh/serverest:2.29.7"
$containerId = $null
$testFilter = "FullyQualifiedName~UsersTests|FullyQualifiedName~ProductsTests|FullyQualifiedName~CartsTests|FullyQualifiedName~LoginTests|FullyQualifiedName~PaymentTests|FullyQualifiedName~TypedApiHappyPathTests"
$resultsFileName = "serverest-$([guid]::NewGuid().ToString('N')).trx"
$resultsPath = Join-Path ([System.IO.Path]::GetTempPath()) $resultsFileName
if (-not [string]::IsNullOrWhiteSpace($Filter)) {
    $testFilter = "($testFilter)&($Filter)"
}
$settingNames = @("URL", "URL_MOCK", "NAME", "AUTHENTICATOR_USER", "AUTHENTICATOR_PASSWORD", "USER_ID", "TOKEN")
$previousSettings = @{}
foreach ($name in $settingNames) {
    $previousSettings[$name] = [Environment]::GetEnvironmentVariable($name, "Process")
}

try {
    $containerId = docker run --detach --rm --publish "127.0.0.1::3000" $image
    if ($LASTEXITCODE -ne 0 -or -not $containerId) {
        throw "Failed to start the isolated ServeRest container."
    }

    $binding = docker port $containerId "3000/tcp"
    if ($LASTEXITCODE -ne 0 -or $binding -notmatch '^127\.0\.0\.1:(\d+)$') {
        throw "Could not determine the isolated ServeRest port."
    }

    $env:URL = "http://127.0.0.1:$($Matches[1])/"
    [Environment]::SetEnvironmentVariable("URL_MOCK", $null, "Process")
    $ready = $false
    for ($attempt = 0; $attempt -lt 60; $attempt++) {
        try {
            Invoke-RestMethod -Uri ($env:URL + "usuarios") -TimeoutSec 2 | Out-Null
            $ready = $true
            break
        }
        catch {
            $exception = $_.Exception
            if ($exception -is [System.Net.WebException] -or
                $exception.GetType().FullName -eq "System.Net.Http.HttpRequestException" -or
                $exception -is [System.Threading.Tasks.TaskCanceledException]) {
                Start-Sleep -Seconds 1
            }
            else {
                throw
            }
        }
    }
    if (-not $ready) {
        throw "The isolated ServeRest container did not become ready."
    }

    $env:NAME = "Local Test Administrator"
    $env:AUTHENTICATOR_USER = "local-admin-$([guid]::NewGuid().ToString('N'))@example.com"
    $env:AUTHENTICATOR_PASSWORD = [guid]::NewGuid().ToString("N")
    $userBody = @{
        nome = $env:NAME
        email = $env:AUTHENTICATOR_USER
        password = $env:AUTHENTICATOR_PASSWORD
        administrador = "true"
    } | ConvertTo-Json
    $user = Invoke-RestMethod -Uri ($env:URL + "usuarios") -Method Post `
        -ContentType "application/json" -Body $userBody
    $env:USER_ID = $user._id
    $loginBody = @{
        email = $env:AUTHENTICATOR_USER
        password = $env:AUTHENTICATOR_PASSWORD
    } | ConvertTo-Json
    $env:TOKEN = (Invoke-RestMethod -Uri ($env:URL + "login") -Method Post `
        -ContentType "application/json" -Body $loginBody).authorization

    dotnet test (Join-Path $repositoryRoot "RestSharpNunitApiTesting.sln") --no-restore `
        --filter $testFilter --logger "trx;LogFileName=$resultsFileName" --results-directory ([System.IO.Path]::GetTempPath()) `
        -- NUnit.NumberOfTestWorkers=0
    if ($LASTEXITCODE -ne 0) {
        throw "The isolated ServeRest test run failed."
    }
    if (-not (Test-Path -LiteralPath $resultsPath)) {
        throw "The test run did not produce results."
    }
    [xml]$results = Get-Content -LiteralPath $resultsPath -Raw
    $counters = $results.SelectSingleNode("//*[local-name()='ResultSummary']/*[local-name()='Counters']")
    if ($null -eq $counters -or [int]$counters.total -eq 0) {
        throw "No isolated ServeRest tests matched the filter."
    }
}
finally {
    foreach ($name in $settingNames) {
        [Environment]::SetEnvironmentVariable($name, $previousSettings[$name], "Process")
    }
    if ($containerId) {
        docker stop $containerId | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to stop the isolated ServeRest container $containerId."
        }
    }
    if (Test-Path -LiteralPath $resultsPath) {
        Remove-Item -LiteralPath $resultsPath
    }
}
