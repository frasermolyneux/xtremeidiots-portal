CREATE TABLE [dbo].[UserProfileClaims]
(
	[UserProfileClaimId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT newsequentialid(), 
	[UserProfileId] UNIQUEIDENTIFIER NOT NULL,
	[SystemGenerated] BIT NOT NULL DEFAULT 0, 
    [ClaimType] NVARCHAR(128) NOT NULL, 
    [ClaimValue] NVARCHAR(MAX) NOT NULL, 
    CONSTRAINT [FK_dbo.UserProfileClaims_dbo.UserProfiles_Id] FOREIGN KEY ([UserProfileId]) REFERENCES [dbo].[UserProfiles] ([UserProfileId]),
)
