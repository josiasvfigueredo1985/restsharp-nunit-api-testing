$ErrorActionPreference = "Stop"

$openssl = Get-Command openssl -ErrorAction SilentlyContinue
if (-not $openssl) {
    throw "OpenSSL CLI was not found. Install OpenSSL, then rerun this script."
}

$certificateDirectory = [System.IO.Path]::GetFullPath(
    (Join-Path $PSScriptRoot "..\docker\mysql\certs"))
New-Item -ItemType Directory -Path $certificateDirectory -Force | Out-Null

$caKey = Join-Path $certificateDirectory "ca-key.pem"
$caCertificate = Join-Path $certificateDirectory "ca.pem"
$serverKey = Join-Path $certificateDirectory "server-key.pem"
$serverRequest = Join-Path $certificateDirectory "server.csr"
$serverCertificate = Join-Path $certificateDirectory "server-cert.pem"
$extensions = Join-Path $env:TEMP "restsharp-mysql-server-extensions.cnf"

if ((Test-Path $caKey) -or (Test-Path $caCertificate) -or (Test-Path $serverKey) -or
    (Test-Path $serverCertificate)) {
    throw "Local MySQL certificates already exist in '$certificateDirectory'. Remove them explicitly before regenerating."
}

@"
basicConstraints=critical,CA:FALSE
keyUsage=critical,digitalSignature,keyEncipherment
extendedKeyUsage=serverAuth
subjectAltName=DNS:localhost,IP:127.0.0.1
subjectKeyIdentifier=hash
authorityKeyIdentifier=keyid,issuer
"@ | Set-Content -LiteralPath $extensions -Encoding ascii

try {
    & $openssl.Source req -x509 -newkey rsa:3072 -sha256 -days 3650 -nodes `
        -keyout $caKey -out $caCertificate -subj "/CN=RestSharp Local MySQL CA" `
        -addext "basicConstraints=critical,CA:TRUE" `
        -addext "keyUsage=critical,keyCertSign,cRLSign"
    if ($LASTEXITCODE -ne 0) {
        throw "OpenSSL failed to create the local CA."
    }

    & $openssl.Source req -newkey rsa:2048 -sha256 -nodes `
        -keyout $serverKey -out $serverRequest -subj "/CN=localhost"
    if ($LASTEXITCODE -ne 0) {
        throw "OpenSSL failed to create the MySQL server certificate request."
    }

    & $openssl.Source x509 -req -in $serverRequest -CA $caCertificate -CAkey $caKey `
        -CAcreateserial -out $serverCertificate -days 825 -sha256 -extfile $extensions
    if ($LASTEXITCODE -ne 0) {
        throw "OpenSSL failed to sign the MySQL server certificate."
    }

    & $openssl.Source verify -CAfile $caCertificate -verify_hostname localhost $serverCertificate
    if ($LASTEXITCODE -ne 0) {
        throw "The generated certificate did not validate for localhost."
    }

    & $openssl.Source verify -CAfile $caCertificate -verify_ip 127.0.0.1 $serverCertificate
    if ($LASTEXITCODE -ne 0) {
        throw "The generated certificate did not validate for 127.0.0.1."
    }
}
finally {
    Remove-Item -LiteralPath $extensions -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $serverRequest -ErrorAction SilentlyContinue
}

Write-Host "Local MySQL certificates created in '$certificateDirectory'. Keep the private keys local."
