FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./nuget.config .
COPY ["DatabaseReportingSystem.API/DatabaseReportingSystem.API.csproj", "DatabaseReportingSystem.API/"]
COPY ["DatabaseReportingSystem.Agency/DatabaseReportingSystem.Agency.csproj", "DatabaseReportingSystem.Agency/"]
COPY ["DatabaseReportingSystem.Shared/DatabaseReportingSystem.Shared.csproj", "DatabaseReportingSystem.Shared/"]
COPY ["DatabaseReportingSystem.Vector/DatabaseReportingSystem.Vector.csproj", "DatabaseReportingSystem.Vector/"]
RUN dotnet restore "DatabaseReportingSystem.API/DatabaseReportingSystem.API.csproj" --configfile nuget.config --ignore-failed-sources
COPY . .
WORKDIR "/src/DatabaseReportingSystem.API"
RUN dotnet build "DatabaseReportingSystem.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DatabaseReportingSystem.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY certs /etc/ssl/drs
RUN chmod -R 644 /etc/ssl/drs/*
ENTRYPOINT ["dotnet", "DatabaseReportingSystem.API.dll"]
