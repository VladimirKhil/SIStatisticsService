param (
    [string]$version = "1.0.0",
    [string]$apikey = ""
)

dotnet pack src\SIStatisticsService.Contract\SIStatisticsService.Contract.csproj -c Release /property:Version=$version
dotnet pack src\SIStatisticsService.Client\SIStatisticsService.Client.csproj -c Release /property:Version=$version
dotnet nuget push bin\.Release\SIStatisticsService.Contract\VKhil.SIStatisticsService.Contract.$version.nupkg --api-key $apikey --source https://api.nuget.org/v3/index.json
dotnet nuget push bin\.Release\SIStatisticsService.Client\VKhil.SIStatisticsService.Client.$version.nupkg --api-key $apikey --source https://api.nuget.org/v3/index.json