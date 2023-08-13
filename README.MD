
# matts - "Matthew's ATS"

  

### Local Dev Environment Setup

### DOCKER

Make sure you have [Powershell 7](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows?view=powershell-7.3&viewFallbackFrom=powershell-7&WT.mc_id=THOMASMAURER-blog-thmaure).

  

#### First Time

__As Administrator__, run these powershell commands to setup your environment. Once setup, you can start and stop the container to access the application.

If you can't run as admin, change the certstore location to `CurrentUser`.

```powershell
# Cert Path Setup
$certsFolder = "$(If ($env:HOME -ne $null) {$env:HOME} Else {$env:USERPROFILE})\.devcerts"
New-Item -Force -ItemType "directory" -Path $certsFolder
$certKeyPath = "$certsFolder\matts-localhost.pfx"

# Create a self-signed Cert
$cert = New-SelfSignedCertificate -DnsName "localhost" -FriendlyName "localhost development certificate" -NotAfter (Get-Date).AddYears(15) -CertStoreLocation "cert:\LocalMachine\My"
$password = Get-Credential -Message 'Enter Password' -UserName 'SSL Certificate'

# Export the self-signed cert
$cert | Export-PfxCertificate -FilePath $certKeyPath -Password $password.Password
$rootCert = $(Import-PfxCertificate -FilePath $certKeyPath -CertStoreLocation 'Cert:\LocalMachine\Root' -Password $password.Password)

# Place your cert's password in an env file for the container
"ASPNETCORE_Kestrel__Certificates__Default__Password=" | Out-File -FilePath docker.development.secret.env -NoNewline
ConvertFrom-SecureString -SecureString $password.Password -AsPlainText | Out-File -FilePath docker.development.secret.env -Append

# Setup the user-secrets stuff
dotnet user-secrets set "Neo4J:User"  "neo4j"
dotnet user-secrets set "Neo4J:Password"  "changeme"
$stream = [IO.MemoryStream]::new([Text.Encoding]::UTF8.GetBytes($(Get-Random).toString()))
$sha = Get-FileHash -InputStream $stream -Algorithm SHA256
$bytes = [byte[]] -split ($sha.Hash -replace '..', '0x$& ')
$key = [Convert]::ToBase64String($bytes)
dotnet user-secrets set "Jwt:SigningKey"  $key
dotnet user-jwts create

# Compose container using the override profile for development
$env:MATTS_DEVCERT_FOLDER = $certsFolder
docker-compose --env-file ./docker.development.env -f docker-compose.yml -f docker-compose.development.yml up -d

```

__While Neo4J is running,__ connect to it via web browser, `http://localhost:7474/browser/`, to configure the constraints. This only has to be done once:

```
CREATE CONSTRAINT FOR(a:Applicant) REQUIRE a.uuid IS UNIQUE
CREATE CONSTRAINT FOR(j:Job) REQUIRE j.uuid IS UNIQUE
CREATE CONSTRAINT FOR(e:Employer) REQUIRE e.uuid IS UNIQUE
CREATE CONSTRAINT FOR(u:User) REQUIRE u.userName IS UNIQUE
```

#### Subsequent Rebuilds

This is all that's required once the initial secrets setup & cert generation has been performed.

```powershell
$certsFolder = "$(If ($env:HOME -ne $null) {$env:HOME} Else {$env:USERPROFILE})\.devcerts"
$env:MATTS_DEVCERT_FOLDER = $certsFolder
docker-compose --env-file ./docker.development.env -f docker-compose.yml -f docker-compose.development.yml up --build matts
```

### NON-DOCKER

You need to have Neo4J setup. Once it's setup, you just start and stop it using Docker Desktop.

First, setup the container...

```powershell
docker-compose --env-file ./docker.development.env -f docker-compose.yml -f docker-compose.development.yml up neo4j -d
```

Next, connect to `http://localhost:7474/browser/` and connect to it via web browser to configure the constraints. This only has to be done once:

```
CREATE CONSTRAINT FOR(a:Applicant) REQUIRE a.uuid IS UNIQUE
CREATE CONSTRAINT FOR(j:Job) REQUIRE j.uuid IS UNIQUE
CREATE CONSTRAINT FOR(e:Employer) REQUIRE e.uuid IS UNIQUE
CREATE CONSTRAINT FOR(u:User) REQUIRE u.userName IS UNIQUE
```

__(One time only)__ Next, you need to setup your secrets:
```powershell
dotnet user-secrets set "Neo4J:User"  "neo4j"
dotnet user-secrets set "Neo4J:Password"  "changeme"
$stream = [IO.MemoryStream]::new([Text.Encoding]::UTF8.GetBytes($(Get-Random).toString()))
$sha = Get-FileHash -InputStream $stream -Algorithm SHA256
$bytes = [byte[]] -split ($sha.Hash -replace '..', '0x$& ')
$key = [Convert]::ToBase64String($bytes)
dotnet user-secrets set "Jwt:SigningKey"  $key
dotnet user-jwts create
```

Finally, all you need to do to run the app is to execute **`dotnet run`** from the project root directory.
