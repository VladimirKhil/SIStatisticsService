FROM mcr.microsoft.com/dotnet/runtime-deps:7.0-alpine AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/SIStatisticsService.Contract/SIStatisticsService.Contract.csproj", "src/SIStatisticsService.Contract/"]
COPY ["src/SIStatisticsService.Database/SIStatisticsService.Database.csproj", "src/SIStatisticsService.Database/"]
COPY ["src/SIStatisticsService/SIStatisticsService.csproj", "src/SIStatisticsService/"]
RUN dotnet restore "src/SIStatisticsService/SIStatisticsService.csproj" -r linux-musl-x64
COPY . .
WORKDIR "/src/src/SIStatisticsService"
RUN dotnet publish "SIStatisticsService.csproj" -p:PublishSingleFile=true -r linux-musl-x64 -p:PublishTrimmed=true -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["./SIStatisticsService", "--urls", "http://*:5000"]