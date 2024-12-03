#!/bin/bash

# building project

echo Checking for args..
if [ $# -lt 2 ]; then
    echo No arg provided.. Please provide Debug or Release and assembly version
    exit 1
fi

# check for build output dir
if [ -d "BuildOutput" ]
then
    echo "BuildOutput dir already exists... continuing the build"
else
    mkdir -p BuildOutput
fi

if [[ $2 =~ ^[0-9]+\.[0-9]+\.[0-9]+ ]]; then
    echo valid assembly version
    version=$2
else
    echo invalid assembly version, using 1.0.0.0
    version=1.0.0.0
fi

# build
echo "build project..."
dotnet build 'OPC UA Nodeset WebAPI/OPC UA Nodeset WebAPI.csproj' -p:Configuration=$config -p:Version=$version -o BuildOutput/
