# SIStatisticsService

Service for collecting SIGame games and packages statistics.

Service accept game and package info. It returns information about latest finished games and overall game statistic for some platform.

# Build

    dotnet build

# Run

## Docker


    docker run -p 5000:5000 vladimirkhil/sistatisticsservice:1.0.0


## Helm


    dependencies:
    - name: sistatistics
      version: "1.0.0"
      repository: "https://vladimirkhil.github.io/SIStatisticsService/helm/repo"