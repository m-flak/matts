# matts - "Matthew's ATS"

### Neo4J Local Setup
Run:
* `docker compose -f docker-compose.yml up neo4j`
* `dotnet user-secrets set "Neo4J:User" "neo4j"`
* `dotnet user-secrets set "Neo4J:Password" "changeme"`

Examine `cypher_queries.txt` file and run those commands.

### JWT Secret
Run these powershell commands:

```Powershell
$stream = [IO.MemoryStream]::new([Text.Encoding]::UTF8.GetBytes($(Get-Random).toString()))
$sha = Get-FileHash -InputStream $stream -Algorithm SHA256
dotnet user-secrets set "Jwt:SigningKey" $sha.Hash
dotnet user-jwts create
```
