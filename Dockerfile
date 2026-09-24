FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

WORKDIR /app

EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY ["AsyncHealthChecker.Api/AsyncHealthChecker.Api.csproj", "AsyncHealthChecker.Api/"]
COPY ["AsyncHealthChecker.Application/AsyncHealthChecker.Application.csproj", "AsyncHealthChecker.Application/"]
COPY ["AsyncHealthChecker.Domain/AsyncHealthChecker.Domain.csproj", "AsyncHealthChecker.Domain/"]
COPY ["AsyncHealthChecker.Infrastructure/AsyncHealthChecker.Infrastructure.csproj", "AsyncHealthChecker.Infrastructure/"]

RUN dotnet restore "AsyncHealthChecker.Api/AsyncHealthChecker.Api.csproj"

COPY . .

WORKDIR "/src/AsyncHealthChecker.Api"

RUN dotnet build "AsyncHealthChecker.Api.csproj" -c Release -o /app/build

FROM build AS publish

RUN dotnet publish "AsyncHealthChecker.Api.csproj" -c Release -o /app/publish

FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "AsyncHealthChecker.Api.dll"]