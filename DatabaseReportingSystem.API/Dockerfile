FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DatabaseReportingSystem/DatabaseReportingSystem.API.csproj", "DatabaseReportingSystem.API/"]
RUN dotnet restore "DatabaseReportingSystem.API/DatabaseReportingSystem.API.csproj"
COPY . .
WORKDIR "/src/DatabaseReportingSystem"
RUN dotnet build "DatabaseReportingSystem.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DatabaseReportingSystem.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DatabaseReportingSystem.API.dll"]
