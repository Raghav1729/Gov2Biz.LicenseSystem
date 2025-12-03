FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Gov2Biz.PaymentService/Gov2Biz.PaymentService.csproj", "Gov2Biz.PaymentService/"]
COPY ["Gov2Biz.Shared/Gov2Biz.Shared.csproj", "Gov2Biz.Shared/"]
RUN dotnet restore "Gov2Biz.PaymentService/Gov2Biz.PaymentService.csproj"
COPY . .
WORKDIR "/src/Gov2Biz.PaymentService"
RUN dotnet build "Gov2Biz.PaymentService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Gov2Biz.PaymentService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gov2Biz.PaymentService.dll"]
