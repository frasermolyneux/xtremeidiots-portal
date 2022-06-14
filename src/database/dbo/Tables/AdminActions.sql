CREATE TABLE [dbo].[AdminActions] (
    [AdminActionId]  UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [PlayerId] UNIQUEIDENTIFIER NOT NULL,
    [UserProfileId] UNIQUEIDENTIFIER NULL,
    [ForumTopicId] INT DEFAULT NULL NULL,
    [Type] INT NOT NULL,
    [Text] NVARCHAR (MAX) NOT NULL,
    [Created] DATETIME NOT NULL,
    [Expires] DATETIME NULL,
    CONSTRAINT [PK_dbo.AdminActions] PRIMARY KEY CLUSTERED ([AdminActionId] ASC),
    CONSTRAINT [FK_dbo.AdminActions_dbo.UserProfiles_UserProfileId] FOREIGN KEY ([UserProfileId]) REFERENCES [dbo].[UserProfiles] ([UserProfileId]),
    CONSTRAINT [FK_dbo.AdminActions_dbo.Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Players] ([PlayerId])
);

GO
CREATE NONCLUSTERED INDEX [IX_PlayerId]
    ON [dbo].[AdminActions]([PlayerId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_UserProfileId]
    ON [dbo].[AdminActions]([UserProfileId] ASC);
