FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Gov2Biz.Gateway/Gov2Biz.Gateway.csproj", "src/Gov2Biz.Gateway/"]
COPY ["src/Gov2Biz.Shared/Gov2Biz.Shared.csproj", "src/Gov2Biz.Shared/"]
RUN dotnet restore "src/Gov2Biz.Gateway/Gov2Biz.Gateway.csproj"
COPY . .
RUN dotnet publish "src/Gov2Biz.Gateway/Gov2Biz.Gateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Gov2Biz.Gateway.dll"]
