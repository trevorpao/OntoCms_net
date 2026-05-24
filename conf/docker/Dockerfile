FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/public/OntoCms.Web.csproj src/public/
RUN dotnet restore src/public/OntoCms.Web.csproj

COPY src/public/ src/public/
COPY src/conventions/ src/conventions/
COPY src/Modules/ src/Modules/
COPY src/theme/default/ src/theme/default/
COPY document/sql/ document/sql/
RUN dotnet publish src/public/OntoCms.Web.csproj -c Release -o /app/publish/web /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN apt-get update \
	&& apt-get install -y --no-install-recommends openssl \
	&& rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish/web .
COPY --from=build /src/document/sql ./sql
COPY conf/dotnet/entrypoint.sh /entrypoint.sh

RUN chmod +x /entrypoint.sh

EXPOSE 8080 8443

ENTRYPOINT ["/entrypoint.sh"]