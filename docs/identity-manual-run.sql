IF (NOT EXISTS(SELECT * FROM sys.database_principals WHERE [name] = 'sg-sql-portal-prd-identitydb-readers'))  
BEGIN  
	PRINT 'Adding user: sg-sql-portal-prd-identitydb-readers to [db_datareader]'
	CREATE USER [sg-sql-portal-prd-identitydb-readers] FROM EXTERNAL PROVIDER
	ALTER ROLE [db_datareader] ADD MEMBER [sg-sql-portal-prd-identitydb-readers]
	SELECT * FROM sys.database_principals WHERE [name] = 'sg-sql-portal-prd-identitydb-readers'
END  

IF (NOT EXISTS(SELECT * FROM sys.database_principals WHERE [name] = 'sg-sql-portal-prd-identitydb-writers'))  
BEGIN  
	PRINT 'Adding user: sg-sql-portal-prd-identitydb-writers to [db_datawriter]'
	CREATE USER [sg-sql-portal-prd-identitydb-writers] FROM EXTERNAL PROVIDER
	ALTER ROLE [db_datawriter] ADD MEMBER [sg-sql-portal-prd-identitydb-writers]
	SELECT * FROM sys.database_principals WHERE [name] = 'sg-sql-portal-prd-identitydb-writers'
END  