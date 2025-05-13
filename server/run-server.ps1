# Check if -sc flag is provided
if ($args -contains "-sc") {
    Write-Host "Running server with StyleCop enabled..."
    dotnet run --property:StyleCopEnabled=true
} else {
    Write-Host "Running server without StyleCop..."
    dotnet run
} 