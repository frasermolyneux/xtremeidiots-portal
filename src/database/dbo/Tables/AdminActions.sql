CREATE TABLE [dbo].[AdminActions] (
    [AdminActionId]     UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [UserProfileId]     UNIQUEIDENTIFIER NULL,
    [PlayerId]          UNIQUEIDENTIFIER NULL,
    [ForumTopicId]      INT              DEFAULT ((0)) NOT NULL,
    [Type]              INT              NOT NULL,
    [Text]              NVARCHAR (MAX)   NULL,
    [Created]           DATETIME         NOT NULL,
    [Expires]           DATETIME         NULL,
    CONSTRAINT [PK_dbo.AdminActions] PRIMARY KEY CLUSTERED ([AdminActionId] ASC),
    CONSTRAINT [FK_dbo.AdminActions_dbo.UserProfiles_Id] FOREIGN KEY ([UserProfileId]) REFERENCES [dbo].[UserProfiles] ([UserProfileId]),
    CONSTRAINT [FK_dbo.AdminActions_dbo.Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Players] ([PlayerId])
);

GO
CREATE NONCLUSTERED INDEX [IX_Players_PlayerId]
    ON [dbo].[AdminActions]([PlayerId] ASC);
