FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Gov2Biz.NotificationService/Gov2Biz.NotificationService.csproj", "Gov2Biz.NotificationService/"]
COPY ["Gov2Biz.Shared/Gov2Biz.Shared.csproj", "Gov2Biz.Shared/"]
RUN dotnet restore "Gov2Biz.NotificationService/Gov2Biz.NotificationService.csproj"
COPY . .
WORKDIR "/src/Gov2Biz.NotificationService"
RUN dotnet build "Gov2Biz.NotificationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Gov2Biz.NotificationService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gov2Biz.NotificationService.dll"]
