#!/bin/bash

# Build and Generate Documentation Script for FareCalculator

# Set up .NET environment
export PATH="$PATH:/home/ubuntu/.dotnet:/home/ubuntu/.dotnet/tools"
export DOTNET_ROOT="/home/ubuntu/.dotnet"

echo -e "\033[32mBuilding FareCalculator Documentation...\033[0m"

# Build the main project to generate XML documentation
echo -e "\033[33mBuilding main project...\033[0m"
dotnet build src/FareCalculator/FareCalculator.csproj --configuration Release
if [ $? -ne 0 ]; then
    echo -e "\033[31mFailed to build main project\033[0m"
    exit 1
fi

# Build the test project to generate test documentation
echo -e "\033[33mBuilding test project...\033[0m"
dotnet build tests/FareCalculator.Tests/FareCalculator.Tests.csproj --configuration Release
if [ $? -ne 0 ]; then
    echo -e "\033[31mFailed to build test project\033[0m"
    exit 1
fi

# Generate documentation with DocFX
echo -e "\033[33mGenerating documentation with DocFX...\033[0m"
docfx build docfx.json
if [ $? -ne 0 ]; then
    echo -e "\033[31mFailed to generate documentation\033[0m"
    exit 1
fi

echo -e "\033[32mDocumentation generated successfully!\033[0m"
echo -e "\033[36mFiles available in: _site/\033[0m"
echo ""
echo -e "\033[37mTo serve the documentation locally, run:\033[0m"
echo -e "\033[90m  docfx serve _site\033[0m"
echo ""
echo -e "\033[37mOr to build and serve in one command:\033[0m"
echo -e "\033[90m  docfx docfx.json --serve\033[0m"