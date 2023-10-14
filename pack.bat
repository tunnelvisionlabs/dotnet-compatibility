@ECHO OFF

dotnet pack -c Release -o ./packed CompatibilityChecker.Library/CompatibilityChecker.Library.csproj

dotnet pack -c Release -o ./packed CompatibilityChecker/CompatibilityChecker.csproj