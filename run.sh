#!/bin/bash

# Run script for Semantic Kernel Clothing Analysis

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo "Error: .NET SDK is not installed or not in the PATH"
    echo "Please install .NET 9.0 SDK or later from https://dotnet.microsoft.com/download"
    exit 1
fi

# Check environment variables
if [ -z "$AZURE_OPENAI_API_KEY" ]; then
    echo "Warning: AZURE_OPENAI_API_KEY environment variable is not set"
    echo "Please set your Azure OpenAI API key:"
    echo "export AZURE_OPENAI_API_KEY=\"your-api-key-here\""
    echo ""
    read -p "Would you like to continue anyway? (y/n): " -n 1 -r
    echo ""
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
fi

# Create necessary directories if they don't exist
mkdir -p clothes/TShirt clothes/Sweater clothes/Jeans results

# Check if sample images exist, or create placeholder text files
if [ ! -f "clothes/TShirt/tshirt.jpg" ]; then
    echo "No sample images found. Creating placeholder files."
    echo "Sample T-shirt image" > "clothes/TShirt/tshirt.txt"
    echo "Sample Sweater image" > "clothes/Sweater/sweater.txt"
    echo "Sample Jeans image" > "clothes/Jeans/jeans.txt"
    echo "Please add your own image files (.jpg, .png) to the clothes subdirectories."
fi

# Build and run the application
echo "Building and running the application..."
cd temp
dotnet build
dotnet run

echo "Application execution completed."
