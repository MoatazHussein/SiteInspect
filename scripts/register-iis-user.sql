USE [master];

DECLARE @Database sysname = N'SiteInspect';
DECLARE @User sysname = N'IIS APPPOOL\SiteInspect';

DECLARE @Sql nvarchar(max) =
    N'USE ' + QUOTENAME(@Database) + N';
CREATE LOGIN ' + QUOTENAME(@User) + N' FROM WINDOWS;
CREATE USER ' + QUOTENAME(@User) + N' FOR LOGIN ' + QUOTENAME(@User) + N';
ALTER ROLE [db_datareader] ADD MEMBER ' + QUOTENAME(@User) + N';
ALTER ROLE [db_datawriter] ADD MEMBER ' + QUOTENAME(@User) + N';';

EXEC sys.sp_executesql @Sql;
