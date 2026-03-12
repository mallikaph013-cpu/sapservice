-- Run with sqlcmd (or SSMS with SQLCMD Mode enabled).
USE [master];
GO

:r .\sqlserver-reset-create.sql

USE [SAP_Service];
GO

:r .\sqlserver-create-from-dbcontext-v2.sql

:r .\sqlite-to-sqlserver-inserts.sql
