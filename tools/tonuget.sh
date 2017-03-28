#!/bin/sh

dotnet pack --configuration release src/NValidate.csproj
nuget setApiKey $1 -source https://www.nuget.org
nuget push src/bin/release/NValidate.*.nupkg -Source https://www.nuget.org/api/v2/package
