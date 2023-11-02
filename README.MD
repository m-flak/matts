# matts - "Matthew's ATS"

__Matthew's ATS__ is a functional, single-tenant, and cloud-native web application serving as an Applicant Tracking System. 

As an employer:
- Post new jobs, fill them, or close them!
- Schedule interviews with prospective candidates and download the interview to your calendar!
- View and download every applicant's resume with the click of a button!

As a job seeker:
- View all open, unfilled positions!
- Apply to jobs with your resume and put your best foot forward!

### Azure Setup
This project consists of two Azure components. 

The first is the application itself. It is an Azure Container Instances (ACI) group.

The second is the companion `matts.AzFunctions` project for Azure Functions used by the application.

##### ACI Group Managed Identity & Permissions
The ACI Group will need a system assigned managed identity with permissions to access the key vault, app config store, storage account, and the blob container (important!). 

The key vault is also used in the GH Action pipeline. A service account will need to be created for your Azure Resource Group. Refer to the project's GitHub Actions folder for more details.

1. __Key Vault Permissions__ <br> ![KEY VAULT SECRETS USER](https://github.com/m-flak/matts/assets/35634280/7be9d12e-dc02-435f-93fc-5b0236442359)
2. __App Config Permissions__ <br> ![APP CONFIGURATION DATA READER](https://github.com/m-flak/matts/assets/35634280/83c49b25-1fa3-4b72-97e0-c8b1102a9e17)
3. __Storage Account Permissions__ <br> ![READER AND DATA ACCESS](https://github.com/m-flak/matts/assets/35634280/0c01d569-68c3-44d3-94ab-9906a53e8299) <br> ![STORAGE BLOB DATA READER and STORAGE BLOB DELEGATOR](https://github.com/m-flak/matts/assets/35634280/e04cada2-f3d5-4001-8277-6cd662a24fd0)
4. __Blob Container Permissions__ <br> ![STORAGE BLOB DATA OWNER](https://github.com/m-flak/matts/assets/35634280/3225e66c-2726-476e-999a-b4073b2779c6)

##### Container Setup
For the nitty-gritty on the container setup, see the `.azure/template.json` file. Since ARM is being used instead of docker-compose for deployment, ensure what's in the docker-compose file is mirrored in the ARM template.
Each volume for a container will need to be assigned an Azure File Share. It's share per volume, apparently.

These settings need to be in the configuration store. The application will filter by its prefix and the build environment that it was built against.
![app config keys](https://github.com/m-flak/matts/assets/35634280/d265b90f-e239-4f49-9cf0-f6782e8707b9)

The service endpoint of the App Config itself will need to be placed within the appropriate `appsettings.json` file.

__NOTE: *This screenshot may not contain everything. Refer to the C# classes within the `Configuration` folder as well as the json configuration files.*__

__For HTTPS,__ currently the application utilizes a self-signed certificate. Place a PFX certificate file within the root of the `apphttps` file share. The DnsName of the cert needs to match the deployment url, which will remain the same after the container group is created, even if the group needs to be deleted due to certain prop changes. If you create one first in Azure Portal, be sure to add together the values for memory and cpu of both containers, so you don't have to delete it before a deployment.

After a successful deployment, the application is accessible at the url attached to [the deployment](https://github.com/m-flak/matts/deployments).

### Local Dev Environment Setup

### DOCKER

Make sure you have [Powershell 7](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows?view=powershell-7.3&viewFallbackFrom=powershell-7&WT.mc_id=THOMASMAURER-blog-thmaure).

  

#### First Time

__As Administrator__, run these powershell commands to setup your environment. Once setup, you can start and stop the container to access the application.

If you can't run as admin, change the certstore location to `CurrentUser`.

```powershell
# Cert Path Setup
$certsFolder = "$(If ($env:HOME -ne $null) {$env:HOME+'/.devcerts'} Else {$env:USERPROFILE+'\.devcerts'})"
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

#### Subsequent Rebuilds

This is all that's required once the initial secrets setup & cert generation has been performed.

```powershell
$certsFolder = "$(If ($env:HOME -ne $null) {$env:HOME+'/.devcerts'} Else {$env:USERPROFILE+'\.devcerts'})"
$env:MATTS_DEVCERT_FOLDER = $certsFolder
docker-compose --env-file ./docker.development.env -f docker-compose.yml -f docker-compose.development.yml up --build matts
```

### NON-DOCKER

You need to have Neo4J setup. Once it's setup, you just start and stop it using Docker Desktop.

First, setup the container...

```powershell
docker-compose --env-file ./docker.development.env -f docker-compose.yml -f docker-compose.development.yml up neo4j -d
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

### Azure App Configuration and Blob Storage on Local

You need to setup the connection string. It varies slightly whether using Docker or not.
Be sure to also update the appropriate `appsettings.json` too.

### DOCKER

In the `docker.development.secret.env` file created earlier, add the following line:
```
ConnectionStrings__AzureAppConfiguration=<YOUR CONNECTION STRING HERE>
ConnectionStrings__<ServiceName from Blob Config Item>=<YOUR CONNECTION STRING HERE>
```

### NON-DOCKER

Use the user secrets tool.
```
dotnet user-secrets set "ConnectionStrings:AzureAppConfiguration" "<YOUR CONNECTION STRING HERE>"
dotnet user-secrets set "ConnectionStrings:<ServiceName from Blob Config Item>" "<YOUR CONNECTION STRING HERE>"
```
