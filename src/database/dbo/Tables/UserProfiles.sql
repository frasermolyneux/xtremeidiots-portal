CREATE TABLE [dbo].[UserProfiles]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT newsequentialid(), 
    [IdentityOid] NVARCHAR(50) NULL, 
    [XtremeIdiotsForumId] NVARCHAR(50) NULL, 
    [DisplayName] NVARCHAR(50) NULL 
)
