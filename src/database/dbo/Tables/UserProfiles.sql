CREATE TABLE [dbo].[UserProfiles]
(
	[UserProfileId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT newsequentialid(), 
    [IdentityOid] NVARCHAR(50) NULL, 
    [XtremeIdiotsForumId] NVARCHAR(50) NULL, 
    [DemoAuthKey] NVARCHAR(50) NULL,
    [DisplayName] NVARCHAR(128) NULL, 
    [FormattedName] NVARCHAR(128) NULL, 
    [PrimaryGroup] NVARCHAR(128) NULL, 
    [Email] NVARCHAR(128) NULL, 
    [PhotoUrl] NVARCHAR(MAX) NULL, 
    [ProfileUrl] NVARCHAR(MAX) NULL, 
    [TimeZone] NVARCHAR(50) NULL 
)
