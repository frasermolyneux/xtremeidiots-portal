﻿CREATE TABLE [dbo].[UserProfile]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT newsequentialid(), 
    [IdentityOid] NVARCHAR(50) NULL, 
    [XtremeIdiotsForumId] NVARCHAR(50) NULL 
)
