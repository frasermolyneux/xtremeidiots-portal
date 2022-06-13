CREATE TABLE [dbo].[Reports]
(
	[ReportId] UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
	[PlayerId] UNIQUEIDENTIFIER NULL,
    [UserProfileId] UNIQUEIDENTIFIER NULL,
    [GameServerId] UNIQUEIDENTIFIER NULL,
    [GameType] INT DEFAULT 0 NOT NULL,
    [Comments] NVARCHAR (MAX) NULL,
    [Timestamp] DATETIME NOT NULL,
    [AdminUserProfileId] UNIQUEIDENTIFIER NULL,
    [AdminClosingComments] NVARCHAR (MAX) NULL,
    [Closed] BIT DEFAULT 0 NOT NULL,
    [ClosedTimestamp] DATETIME NULL,
    CONSTRAINT [PK_dbo.Reports] PRIMARY KEY CLUSTERED ([ReportId] ASC),
    CONSTRAINT [FK_dbo.Reports_dbo.GameServers_GameServerId] FOREIGN KEY ([GameServerId]) REFERENCES [dbo].[GameServers] ([GameServerId]),
    CONSTRAINT [FK_dbo.Reports_dbo.Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Players] ([PlayerId]),
    CONSTRAINT [FK_dbo.Reports_dbo.UserProfiles_Id] FOREIGN KEY ([UserProfileId]) REFERENCES [dbo].[UserProfiles] ([UserProfileId]),
    CONSTRAINT [FK_dbo.Reports_dbo.AdminUserProfiles_Id] FOREIGN KEY ([AdminUserProfileId]) REFERENCES [dbo].[UserProfiles] ([UserProfileId])
)

GO
CREATE NONCLUSTERED INDEX [IX_GameServer_GameServerId]
    ON [dbo].[Reports]([GameServerId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Players_PlayerId]
    ON [dbo].[Reports]([PlayerId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_UserProfiles_Id]
    ON [dbo].[Reports]([UserProfileId] ASC);