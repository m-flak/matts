FROM node:latest AS node
WORKDIR /ClientApp
COPY ./ClientApp/package.json .
COPY ./ClientApp/package-lock.json .
RUN npm install
COPY ./ClientApp/ . 
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
ENV ASPNETCORE_ENVIRONMENT=Staging
COPY *.sln ./matts/
COPY matts.csproj ./matts/
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
RUN dotnet build matts.csproj -c Release -o build
RUN dotnet build matts.Tests/matts.Tests.csproj -c Release

FROM build AS test  
ARG BuildId=localhost
LABEL test=${BuildId}
WORKDIR /src/matts
RUN dotnet test --no-build -c Release --results-directory /testresults --logger "trx;LogFileName=test_results.trx" /p:CollectCoverage=true /p:CoverletOutputFormat=json%2cCobertura /p:CoverletOutput=/testresults/coverage/ -p:MergeWith=/testresults/coverage/coverage.json matts.Tests/matts.Tests.csproj

FROM build AS publish
WORKDIR /src/matts
RUN dotnet publish -C Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=publish /src/matts/out ./
COPY --from=build-node /ClientApp/build ./ClientApp/build
EXPOSE 80
EXPOSE 443
EXPOSE 5000
ENTRYPOINT ["dotnet", "matts.dll"]
