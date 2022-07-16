IF (NOT EXISTS(SELECT * FROM sys.database_principals WHERE [name] = 'sg-sql-platform-prd-portalidentitydb-prd-readers'))  
BEGIN  
	PRINT 'Adding user: sg-sql-platform-prd-portalidentitydb-prd-readers to [db_datareader]'
	CREATE USER [sg-sql-platform-prd-portalidentitydb-prd-readers] FROM EXTERNAL PROVIDER
	ALTER ROLE [db_datareader] ADD MEMBER [sg-sql-platform-prd-portalidentitydb-prd-readers]
	SELECT * FROM sys.database_principals WHERE [name] = 'sg-sql-platform-prd-portalidentitydb-prd-readers'
END  

IF (NOT EXISTS(SELECT * FROM sys.database_principals WHERE [name] = 'sg-sql-platform-prd-portalidentitydb-prd-writers'))  
BEGIN  
	PRINT 'Adding user: sg-sql-platform-prd-portalidentitydb-prd-writers to [db_datawriter]'
	CREATE USER [sg-sql-platform-prd-portalidentitydb-prd-writers] FROM EXTERNAL PROVIDER
	ALTER ROLE [db_datawriter] ADD MEMBER [sg-sql-platform-prd-portalidentitydb-prd-writers]
	SELECT * FROM sys.database_principals WHERE [name] = 'sg-sql-platform-prd-portalidentitydb-prd-writers'
END  