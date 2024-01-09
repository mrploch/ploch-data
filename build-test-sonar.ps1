dotnet restore Ploch.Common.sln
# dotnet sonarscanner begin /k:"mrploch_ploch-common" /o:"mrploch" /d:sonar.login="971da00b0e8c7ab651e7e564952e84235817529b" /d:sonar.cs.opencover.reportsPaths=**/TestResults/coverage.opencover.xml /d:sonar.host.url="https://sonarcloud.io"
dotnet sonarscanner begin /k:"mrploch_ploch-common" /o:"mrploch" /d:sonar.login="971da00b0e8c7ab651e7e564952e84235817529b" /d:sonar.cs.vscoveragexml.reportsPaths=coverage.xml /d:sonar.host.url="https://sonarcloud.io"
dotnet build Ploch.Common.sln --no-incremental
# dotnet-coverage collect 'dotnet test Ploch.Common.sln' -f xml  -o 'coverage.xml'
# dotnet test .\Ploch.Common.sln -p:CollectCoverage=true -p:CoverletOutput=TestResults/ -p:CoverletOutputFormat=opencover --verbosity normal --logger "trx;LogFileName=TestOutputResults.xml"
# reportgenerator -reports:**/TestResults/coverage.opencover.xml -targetdir:./CoverageReport -reporttypes:OpenCover
dotnet-coverage collect "dotnet test Ploch.Common.sln" -f xml -o "coverage.xml"
dotnet sonarscanner end /d:sonar.login="971da00b0e8c7ab651e7e564952e84235817529b"
