@echo off
REM Run script for Semantic Kernel Clothing Analysis

REM Check if dotnet is installed
where dotnet >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo Error: .NET SDK is not installed or not in the PATH
    echo Please install .NET 9.0 SDK or later from https://dotnet.microsoft.com/download
    exit /b 1
)

REM Check environment variables
if "%AZURE_OPENAI_API_KEY%"=="" (
    echo Warning: AZURE_OPENAI_API_KEY environment variable is not set
    echo Please set your Azure OpenAI API key:
    echo $env:AZURE_OPENAI_API_KEY="your-api-key-here"
    echo.
    set /p CONTINUE="Would you like to continue anyway? (y/n): "
    if /i not "%CONTINUE%"=="y" exit /b 1
)

REM Create necessary directories if they don't exist
if not exist "clothes\TShirt" mkdir "clothes\TShirt"
if not exist "clothes\Sweater" mkdir "clothes\Sweater"
if not exist "clothes\Jeans" mkdir "clothes\Jeans"
if not exist "results" mkdir "results"

REM Check if sample images exist, or create placeholder text files
if not exist "clothes\TShirt\tshirt.jpg" (
    echo No sample images found. Creating placeholder files.
    echo Sample T-shirt image > "clothes\TShirt\tshirt.txt"
    echo Sample Sweater image > "clothes\Sweater\sweater.txt"
    echo Sample Jeans image > "clothes\Jeans\jeans.txt"
    echo Please add your own image files (.jpg, .png) to the clothes subdirectories.
)

REM Build and run the application
echo Building and running the application...
cd temp
dotnet build
dotnet run

echo Application execution completed.
