dotnet build .\BlockConverter\Gulla.Episerver.BlockConverter.csproj -c Release
dotnet pack .\BlockConverter\Gulla.Episerver.BlockConverter.csproj -c Release

move .\BlockConverter\bin\Release\*.nupkg ..\..\Nuget
