FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Gov2Biz.LicenseService/Gov2Biz.LicenseService.csproj", "Gov2Biz.LicenseService/"]
COPY ["Gov2Biz.Shared/Gov2Biz.Shared.csproj", "Gov2Biz.Shared/"]
RUN dotnet restore "Gov2Biz.LicenseService/Gov2Biz.LicenseService.csproj"
COPY . .
WORKDIR "/src/Gov2Biz.LicenseService"
RUN dotnet build "Gov2Biz.LicenseService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Gov2Biz.LicenseService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gov2Biz.LicenseService.dll"]
