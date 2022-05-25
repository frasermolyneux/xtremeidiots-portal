CREATE TABLE [dbo].[UserProfiles]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT newsequentialid(), 
    [IdentityOid] NVARCHAR(50) NULL, 
    [XtremeIdiotsForumId] NVARCHAR(50) NULL, 
    [DisplayName] NVARCHAR(50) NULL, 
    [Title] NVARCHAR(50) NULL, 
    [FormattedName] NVARCHAR(50) NULL, 
    [PrimaryGroup] NVARCHAR(50) NULL, 
    [Email] NVARCHAR(50) NULL, 
    [PhotoUrl] NVARCHAR(256) NULL, 
    [ProfileUrl] NVARCHAR(256) NULL, 
    [TimeZone] NVARCHAR(50) NULL 
)
