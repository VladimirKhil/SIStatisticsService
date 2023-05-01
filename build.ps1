param (
    [string]$tag = "latest"
)

docker build . -f src\SIStatisticsService\Dockerfile -t vladimirkhil/sistatisticsservice:$tag