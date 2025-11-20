
$variants = "linux-x64", "linux-arm64", "win-x64"
$projectName = "ArchiSteamFarm"

$jobs = @();

foreach ($variant in $variants) {
    $buildArgs = '-r', "$variant", '-p:PublishSingleFile=true', '-p:IncludeNativeLibrariesForSelfExtract=true'
    $commonArgs = '-p:PublishTrimmed=true'

    if (($variant -notlike "win-*") -and ($winProjectNames -contains "$projectName")) {
        Write-Output "skip $projectName $variant"
        continue
    }
    else {
        $commonArgs = '-p:PublishTrimmed=true'
    }

    Write-Output "start build $projectName $variant"

    dotnet restore $projectName -p:ContinuousIntegrationBuild=true --nologo
    dotnet publish $projectName -c "Release" -o "./dist/$variant/$projectName" --self-contained=true -p:ContinuousIntegrationBuild=true --no-restore --nologo $commonArgs $buildArgs
}
