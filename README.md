# SIStatisticsService

Service for collecting SIGame games and packages statistics.

Service accepts game and package info. It returns information about latest finished games and overall game statistic for some platform and language.

# Build

    dotnet build

# Run

## Docker


    docker run -p 5000:5000 vladimirkhil/sistatisticsservice:1.0.15


## Helm


    dependencies:
    - name: sistatistics
      version: "1.0.6"
      repository: "https://vladimirkhil.github.io/SIStatisticsService/helm/repo"