@echo off
echo ========================================
echo Simple Process Explorer
echo ========================================
echo.

echo Executando aplicacao...
echo.

start "" "bin\Release\net8.0-windows\ProcessExplorer.exe"

if %errorlevel% neq 0 (
    echo.
    echo ERRO: Executavel nao encontrado.
    echo Execute build.bat primeiro para compilar.
    pause
    exit /b 1
)

echo.
echo Aplicacao iniciada com sucesso!
timeout /t 2
