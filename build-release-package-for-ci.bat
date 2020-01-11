@ECHO OFF

dotnet publish -c Release --framework netcoreapp3.1 --self-contained false -r linux-x64 -o CompatibilityCheckerCLI-ci CompatibilityCheckerCLI\CompatibilityCheckerCLI.csproj 

del /Q ".\CompatibilityCheckerCLI-ci.zip"

pushd CompatibilityCheckerCLI-ci

zip -r -p "..\CompatibilityCheckerCLI-ci.zip" ".\*"

popd

RMDIR /S /Q ".\CompatibilityCheckerCLI-ci"