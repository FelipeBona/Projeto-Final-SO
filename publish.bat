@echo off
echo ========================================
echo Simple Process Explorer - Publish Script
echo Gerando executavel standalone...
echo ========================================
echo.

echo [1/2] Publicando aplicacao...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true
if %errorlevel% neq 0 (
    echo ERRO: Falha ao publicar
    pause
    exit /b 1
)
echo.

echo [2/2] Abrindo pasta de saida...
start "" "bin\Release\net8.0-windows\win-x64\publish"
echo.

echo ========================================
echo Publicacao concluida com sucesso!
echo O executavel esta em: bin\Release\net8.0-windows\win-x64\publish\
echo ========================================
pause
