# RestSharp API Automation

API automation tests built with RestSharp and NUnit.

## Requirements

- .NET 10 SDK
- A Docker-compatible engine, such as Rancher Desktop with `dockerd` or Docker Desktop; Docker Compose is needed for local MySQL
- OpenSSL CLI (to generate local MySQL TLS certificates)

## Configuration

Keep credentials out of `appsettings.json`. Supply required values through environment variables:

- `DB_URL`, `DB_PORT`, `DB_NAME`, `DB_USER`, and `DB_PASSWORD` for MySQL
- `DB_SSL_CA` optionally points to the trusted CA PEM file; leave it unset for a server certificate trusted by the machine
- `AUTHENTICATOR_USER` and `AUTHENTICATOR_PASSWORD` for authentication against a configured API, or `TOKEN` when using an existing token
- `NAME` when the local test administrator needs to be created, and `USER_ID` for scenarios that use a configured user

The database connection always requires TLS with full certificate and host-name validation.

## Local MySQL

The local database uses the official MySQL 8.4 LTS image, a persistent Docker volume, and an initialization script for the `produtos` and `usuarios` tables. The port is published only on loopback. TLS verification remains enabled; the generated server certificate covers `localhost` and `127.0.0.1`.

1. Copy `.env.example` to `.env` and replace both password placeholders with local-only passwords.
2. Generate the local CA and server certificate:

   ```powershell
   .\scripts\New-LocalMySqlCertificates.ps1
   ```

3. Start MySQL:

   ```powershell
   docker compose up -d --build
   ```

4. Configure the test process in the same PowerShell window:

   ```powershell
   $localDb = Get-Content ".\.env" | ConvertFrom-StringData
   $env:DB_URL = "127.0.0.1"
   $env:DB_PORT = $localDb.DB_PORT
   $env:DB_NAME = $localDb.DB_NAME
   $env:DB_USER = $localDb.DB_USER
   $env:DB_PASSWORD = $localDb.DB_PASSWORD
   $env:DB_SSL_CA = (Resolve-Path ".\docker\mysql\certs\ca.pem").Path
   ```

   Then run the desired tests. The schema initialization runs only when Docker creates a new, empty database volume.

## Build and test

Run the ServeRest tests against a temporary isolated API container:

```powershell
dotnet restore RestSharpApiTesting.sln
.\scripts\Test-LocalServeRest.ps1
```

The script starts ServeRest on a loopback-only random port, creates a temporary administrator, runs the user, product, cart, login, and payment tests sequentially, and removes its container afterwards. It uses the official `paulogoncalvesbh/serverest:2.29.7` image; Docker downloads it if necessary.

Payment tests use an in-process, loopback-only mock. It reads the saved request routes, response status codes, and update/delete messages from the Postman collection; the collection itself is not a server. The mock adds in-memory create, read, update, and delete behavior, including a 404 for missing IDs, and resets its data before each payment test. It uses the collection's boolean and numeric payment fields and stops when the test run ends. Set `URL_MOCK` explicitly to use a dedicated external mock instead; that service must support the same stateful contract.

To run an individual test in the same isolated environment:

```powershell
.\scripts\Test-LocalServeRest.ps1 -Filter "FullyQualifiedName~ListRegisteredCarts"
```

The filter is a `dotnet test` filter expression and only selects from the supported isolated test classes.
Individual tests can also be run directly in Test Explorer: when `URL` is not set, the test run starts and removes its own local ServeRest container and generates a temporary administrator. Each test authenticates during setup. If `URL` is set, the configured API and authentication settings are used instead.

`TypedApiHappyPathTests` exercises each API operation using strongly typed request and response models and Bogus-generated data. The existing JSON-template tests remain available. Run only the typed suite with:

```powershell
.\scripts\Test-LocalServeRest.ps1 -Filter "FullyQualifiedName~TypedApiHappyPathTests"
```

Some tests delete all users, products, or payments. When setting `URL` or `URL_MOCK`, use only dedicated test services:

```powershell
dotnet restore RestSharpApiTesting.sln
dotnet build RestSharpApiTesting.sln --no-restore
dotnet test RestSharpApiTesting.sln --no-build
```

## Code quality and pre-commit hook

The .NET SDK's formatter and built-in Microsoft analyzers check this C# project; no separate lint package is required. The baseline enforces formatting, unused imports (IDE0005), and correctness rules for recursive property accessors (CA2011), rethrowing exceptions (CA2200), and exception argument names (CA2208). Build warnings are treated as errors by the quality check. These are general .NET rules, not RestSharp-specific API checks.

After restoring packages, run the checks manually:

```powershell
dotnet format RestSharpApiTesting.sln --no-restore --verify-no-changes --severity warn
dotnet build RestSharpApiTesting.sln --no-restore --no-incremental -warnaserror
```

To apply available formatting and style fixes, run `dotnet format RestSharpApiTesting.sln --no-restore`, then review the changes.

Enable the repository's pre-commit hook once per clone:

```powershell
git config --local core.hooksPath .githooks
```

The hook runs the same checks without starting Docker or running integration tests. It checks the working tree, so stage or stash tracked C# and build-configuration changes and stage or remove untracked ones first; this prevents a partially staged file from passing against different local content. On a new clone, run `dotnet restore RestSharpApiTesting.sln` before committing. Git hooks are local and can be bypassed, so run the same checks in CI as well.

When first adding the hook to Git from Windows, preserve its executable bit for other platforms:

```powershell
git add --chmod=+x .githooks/pre-commit
```

## Test reports

NUnit runs generate an ExtentReports Spark HTML report under
`bin/<Configuration>/<TargetFramework>/Reports/<report-name>_<timestamp>/`.