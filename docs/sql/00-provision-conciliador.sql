-- Ejecutar en 192.168.7.51 con permisos de sysadmin. Una sola vez.
IF DB_ID(N'Conciliador') IS NULL
    CREATE DATABASE Conciliador;
GO
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'conciliador')
    CREATE LOGIN conciliador WITH PASSWORD = N'DEFINIR_PASSWORD_REAL', CHECK_POLICY = OFF;
GO
USE Conciliador;
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'conciliador')
BEGIN
    CREATE USER conciliador FOR LOGIN conciliador;
    ALTER ROLE db_owner ADD MEMBER conciliador;
END
GO
