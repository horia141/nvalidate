#!/bin/sh

dotnet pack --configuration release src/NValidate.csproj
dotnet nuget push src/bin/release/NValidate.*.nupkg --api-key $1 --source https://www.nuget.org/api/v2/package
exit $?
