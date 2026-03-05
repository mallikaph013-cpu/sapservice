USE [master];
GO

IF DB_ID(N'SAP_Service') IS NULL
BEGIN
    CREATE DATABASE [SAP_Service];
    PRINT N'[SUCCESS] Database SAP_Service created.';
END
ELSE
BEGIN
    PRINT N'[INFO] Database SAP_Service already exists. Continue with schema creation.';
END
GO

USE [SAP_Service];
GO

-- Run with sqlcmd mode enabled because :r is a sqlcmd directive.
-- Example: sqlcmd -S localhost -E -i sqlserver-create-program-structure.sql
:r .\sqlserver-create-tables.sql

