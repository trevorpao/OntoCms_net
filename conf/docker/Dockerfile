FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY OntoCms_net.sln ./
COPY src/public/OntoCms.Web.csproj src/public/
COPY src/cli/OntoCms.Cli.csproj src/cli/
RUN dotnet restore OntoCms_net.sln

COPY . .
RUN dotnet publish src/public/OntoCms.Web.csproj -c Release -o /app/publish/web /p:UseAppHost=false
RUN dotnet publish src/cli/OntoCms.Cli.csproj -c Release -o /app/publish/cli /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN apt-get update \
	&& apt-get install -y --no-install-recommends openssl \
	&& rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish/web .
COPY --from=build /app/publish/cli ./cli
COPY --from=build /src/document/sql ./sql
COPY conf/dotnet/entrypoint.sh /entrypoint.sh

RUN chmod +x /entrypoint.sh

EXPOSE 8080 8443

ENTRYPOINT ["/entrypoint.sh"]