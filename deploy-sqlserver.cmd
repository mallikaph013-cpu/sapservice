@echo off
setlocal EnableExtensions EnableDelayedExpansion

REM ================================
REM SQL Server one-click deploy
REM ================================

REM --- Configuration ---
set "SERVER=localhost"
set "DATABASE=SAP_Service"
set "AUTH_MODE=WINDOWS"
set "SQL_USER=sa"
set "SQL_PASSWORD=YourStrong@Passw0rd"

REM --- Script paths ---
set "SCRIPT_DIR=%~dp0"
set "RESET_SCRIPT=%SCRIPT_DIR%sqlserver-reset-create.sql"
set "SCHEMA_SCRIPT=%SCRIPT_DIR%sqlserver-create-from-dbcontext-v2.sql"
set "DATA_SCRIPT=%SCRIPT_DIR%sqlite-to-sqlserver-inserts.sql"

if not exist "%RESET_SCRIPT%" (
    echo [ERROR] Missing reset script: %RESET_SCRIPT%
    exit /b 1
)

if not exist "%SCHEMA_SCRIPT%" (
    echo [ERROR] Missing schema script: %SCHEMA_SCRIPT%
    exit /b 1
)

if not exist "%DATA_SCRIPT%" (
    echo [WARN] Missing data script: %DATA_SCRIPT%
    echo [WARN] Continue with schema only. Generate data script by:
    echo [WARN]   python scripts\export_sqlite_to_sqlserver_inserts.py
)

where sqlcmd >nul 2>&1
if errorlevel 1 (
    echo [ERROR] sqlcmd not found in PATH.
    echo Install SQL Server Command Line Utilities and retry.
    exit /b 1
)

echo ==============================================
echo Deploying database on server: %SERVER%
echo Target database: %DATABASE%
echo Auth mode: %AUTH_MODE%
echo ==============================================

if /I "%AUTH_MODE%"=="SQL" (
    echo [1/3] Reset database...
    sqlcmd -S "%SERVER%" -U "%SQL_USER%" -P "%SQL_PASSWORD%" -b -i "%RESET_SCRIPT%"
    if errorlevel 1 (
        echo [ERROR] Reset step failed.
        exit /b 1
    )

    echo [2/3] Apply schema script...
    sqlcmd -S "%SERVER%" -U "%SQL_USER%" -P "%SQL_PASSWORD%" -d "%DATABASE%" -b -i "%SCHEMA_SCRIPT%"
    if errorlevel 1 (
        echo [ERROR] Schema deployment failed.
        exit /b 1
    )

    if exist "%DATA_SCRIPT%" (
        echo [3/3] Apply data script...
        sqlcmd -S "%SERVER%" -U "%SQL_USER%" -P "%SQL_PASSWORD%" -d "%DATABASE%" -b -i "%DATA_SCRIPT%"
        if errorlevel 1 (
            echo [ERROR] Data deployment failed.
            exit /b 1
        )
    )
) else (
    echo [1/3] Reset database...
    sqlcmd -S "%SERVER%" -E -b -i "%RESET_SCRIPT%"
    if errorlevel 1 (
        echo [ERROR] Reset step failed.
        exit /b 1
    )

    echo [2/3] Apply schema script...
    sqlcmd -S "%SERVER%" -E -d "%DATABASE%" -b -i "%SCHEMA_SCRIPT%"
    if errorlevel 1 (
        echo [ERROR] Schema deployment failed.
        exit /b 1
    )

    if exist "%DATA_SCRIPT%" (
        echo [3/3] Apply data script...
        sqlcmd -S "%SERVER%" -E -d "%DATABASE%" -b -i "%DATA_SCRIPT%"
        if errorlevel 1 (
            echo [ERROR] Data deployment failed.
            exit /b 1
        )
    )
)

echo [SUCCESS] Database reset and deployment completed.
exit /b 0
