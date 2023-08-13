ARG AspNetCoreEnvironment=Development
ARG DotNetBuildConfiguration=Debug

FROM node:18 AS build-node
WORKDIR /ClientApp
COPY ./ClientApp/package.json .
COPY ./ClientApp/package-lock.json .
RUN npm install
COPY ./ClientApp/ . 
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG AspNetCoreEnvironment
ARG DotNetBuildConfiguration
WORKDIR /src
ENV ASPNETCORE_ENVIRONMENT $AspNetCoreEnvironment
COPY *.sln ./matts/
COPY matts.csproj ./matts/
COPY appsettings.json ./matts/
COPY appsettings.$AspNetCoreEnvironment.json ./matts/
COPY Program.cs ./matts/
COPY ./Configuration/* ./matts/Configuration/
COPY ./Constants/* ./matts/Constants/
COPY ./Controllers/* ./matts/Controllers/
COPY ./Daos/* ./matts/Daos/
COPY ./Interfaces/* ./matts/Interfaces/
COPY ./matts.Tests/* ./matts/matts.Tests/
COPY ./Models/* ./matts/Models/
COPY ./Pages/* ./matts/Pages/
COPY ./Properties/* ./matts/Properties/
COPY ./Repositories/* ./matts/Repositories/
COPY ./Services/* ./matts/Services/
COPY ./Utils/* ./matts/Utils/
COPY ./wwwroot/* ./matts/wwwroot/
RUN dotnet restore ./matts/matts.csproj
WORKDIR /src/matts
RUN dotnet build matts.csproj -c $DotNetBuildConfiguration -o build -p:IsDockerBuild=true
RUN dotnet clean matts.Tests/matts.Tests.csproj -c $DotNetBuildConfiguration
RUN dotnet build matts.Tests/matts.Tests.csproj -c $DotNetBuildConfiguration -p:IsDockerBuild=true

FROM build AS test  
ARG DotNetBuildConfiguration
ARG BuildId=localhost
LABEL test=${BuildId}
WORKDIR /src/matts
RUN dotnet test --no-build -c $DotNetBuildConfiguration --results-directory /testresults --logger "trx;LogFileName=test_results.trx" /p:CollectCoverage=true /p:CoverletOutputFormat=json%2cCobertura /p:CoverletOutput=/testresults/coverage/ -p:MergeWith=/testresults/coverage/coverage.json -p:IsDockerBuild=true matts.Tests/matts.Tests.csproj

FROM build AS publish
ARG DotNetBuildConfiguration
WORKDIR /src/matts
COPY --from=build-node /ClientApp/dist ./ClientApp/dist
RUN dotnet publish -c $DotNetBuildConfiguration -o out -p:IsDockerBuild=true

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=publish /src/matts/out ./
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "matts.dll"]
