#!/bin/sh

echo 'Here'
dotnet pack --configuration release src/NValidate.csproj
echo "Packed $?"
nuget setApiKey $1 -source https://www.nuget.org
echo "Set API key $?"
nuget push src/bin/release/NValidate.*.nupkg -Source https://www.nuget.org/api/v2/package
echo "Pushed $?"
