#!/bin/sh

echo 'Here'
dotnet pack --configuration release src/NValidate.csproj
echo "Packed $?"
dotnet nuget push src/bin/release/NValidate.*.nupkg --api-key $1
echo "Pushed $?"
