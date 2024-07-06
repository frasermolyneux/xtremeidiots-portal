IF (NOT EXISTS(SELECT * FROM sys.database_principals WHERE [name] = 'sql-portal-core-prd-portal-web-l6supxzf6itfq-readers'))  
BEGIN  
	PRINT 'Adding user: sql-portal-core-prd-portal-web-l6supxzf6itfq-readers to [db_datareader]'
	CREATE USER [sql-portal-core-prd-portal-web-l6supxzf6itfq-readers] FROM EXTERNAL PROVIDER
	ALTER ROLE [db_datareader] ADD MEMBER [sql-portal-core-prd-portal-web-l6supxzf6itfq-readers]
	SELECT * FROM sys.database_principals WHERE [name] = 'sql-portal-core-prd-portal-web-l6supxzf6itfq-readers'
END  

IF (NOT EXISTS(SELECT * FROM sys.database_principals WHERE [name] = 'sql-portal-core-prd-portal-web-l6supxzf6itfq-writers'))  
BEGIN  
	PRINT 'Adding user: sql-portal-core-prd-portal-web-l6supxzf6itfq-writers to [db_datawriter]'
	CREATE USER [sql-portal-core-prd-portal-web-l6supxzf6itfq-writers] FROM EXTERNAL PROVIDER
	ALTER ROLE [db_datawriter] ADD MEMBER [sql-portal-core-prd-portal-web-l6supxzf6itfq-writers]
	SELECT * FROM sys.database_principals WHERE [name] = 'sql-portal-core-prd-portal-web-l6supxzf6itfq-writers'
END  