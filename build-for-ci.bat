@ECHO OFF

dotnet publish -c Release --framework netcoreapp3.1 --self-contained false -r linux-x64 CompatibilityCheckerCLI\CompatibilityCheckerCLI.csproj 