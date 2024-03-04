ARG AspNetCoreEnvironment=Development
ARG DotNetBuildConfiguration=Debug

FROM node:18 AS build-node
WORKDIR /ClientApp
COPY ./src/matts/ClientApp/package.json .
COPY ./src/matts/ClientApp/package-lock.json .
RUN npm install
COPY ./src/matts/ClientApp/ .
RUN npm run build

FROM zenika/alpine-chrome:with-node AS test-node
LABEL test=node
WORKDIR /ClientApp
COPY --from=build-node --chown=chrome:chrome /ClientApp/ .
RUN npm run test-ci

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG AspNetCoreEnvironment
ARG DotNetBuildConfiguration
WORKDIR /src
ENV ASPNETCORE_ENVIRONMENT $AspNetCoreEnvironment
COPY Directory.* .
COPY ./src/matts/matts.csproj ./matts/
COPY ./src/matts/appsettings.json ./matts/
COPY ./src/matts/appsettings.$AspNetCoreEnvironment.json ./matts/
COPY ./src/matts/*.cs ./matts/
COPY ./src/matts/Configuration/* ./matts/Configuration/
COPY ./src/matts/Constants/* ./matts/Constants/
COPY ./src/matts/Controllers/* ./matts/Controllers/
COPY ./src/matts/Daos/* ./matts/Daos/
COPY ./src/matts/Interfaces/* ./matts/Interfaces/
COPY ./src/matts.Tests/* ./matts.Tests/
COPY ./src/matts/Middleware/* ./matts/Middleware/
COPY ./src/matts/Models/* ./matts/Models/
COPY ./src/matts/Pages/* ./matts/Pages/
COPY ./src/matts/Properties/* ./matts/Properties/
COPY ./src/matts/Repositories/* ./matts/Repositories/
COPY ./src/matts/Services/* ./matts/Services/
COPY ./src/matts/Utils/* ./matts/Utils/
COPY ./src/matts/wwwroot/* ./matts/wwwroot/
WORKDIR /
COPY *.sln .
COPY Directory.* .
RUN dotnet restore
WORKDIR /src/matts
RUN dotnet build matts.csproj -c $DotNetBuildConfiguration -o build -p:IsDockerBuild=true
WORKDIR /src/matts.Tests
RUN dotnet clean matts.Tests.csproj -c $DotNetBuildConfiguration
RUN dotnet build matts.Tests.csproj -c $DotNetBuildConfiguration -p:IsDockerBuild=true

FROM build AS test  
ARG DotNetBuildConfiguration
LABEL test=dotnet
WORKDIR /src/matts.Tests
RUN dotnet test --no-build -c $DotNetBuildConfiguration --results-directory /testresults --logger "trx;LogFileName=test_results.trx" /p:CollectCoverage=true /p:CoverletOutputFormat=json%2cCobertura /p:CoverletOutput=/testresults/coverage/ -p:MergeWith=/testresults/coverage/coverage.json -p:IsDockerBuild=true matts.Tests.csproj

FROM build AS publish
ARG DotNetBuildConfiguration
WORKDIR /src/matts
COPY --from=build-node /ClientApp/dist ./ClientApp/dist
RUN dotnet publish -c $DotNetBuildConfiguration -o out -p:IsDockerBuild=true ./matts.csproj

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=publish /src/matts/out ./
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "matts.dll"]
