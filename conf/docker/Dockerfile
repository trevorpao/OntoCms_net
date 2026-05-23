FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY OntoCms_net.sln ./
COPY src/public/OntoCms.Web.csproj src/public/
RUN dotnet restore OntoCms_net.sln

COPY . .
RUN dotnet publish src/public/OntoCms.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /src/document/sql ./sql

EXPOSE 8080

ENTRYPOINT ["dotnet", "OntoCms.Web.dll"]