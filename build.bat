@echo off
echo ========================================
echo Simple Process Explorer - Build Script
echo ========================================
echo.

echo [1/3] Restaurando dependencias...
dotnet restore
if %errorlevel% neq 0 (
    echo ERRO: Falha ao restaurar dependencias
    pause
    exit /b 1
)
echo.

echo [2/3] Compilando projeto...
dotnet build -c Release
if %errorlevel% neq 0 (
    echo ERRO: Falha na compilacao
    pause
    exit /b 1
)
echo.

echo [3/3] Executando aplicacao...
echo.

start "" "bin\Release\net8.0-windows\ProcessExplorer.exe"

if %errorlevel% neq 0 (
    echo ERRO: Nao foi possivel executar a aplicacao
    pause
    exit /b 1
)

echo.
echo ========================================
echo Aplicacao iniciada com sucesso!
echo ========================================
timeout /t 3
