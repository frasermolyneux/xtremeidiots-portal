﻿CREATE TABLE [dbo].[AdminActions] (
    [AdminActionId]   UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [Type]            INT              NOT NULL,
    [Text]            NVARCHAR (MAX)   NULL,
    [Created]         DATETIME         NOT NULL,
    [Expires]         DATETIME         NULL,
    [UserProfileId] UNIQUEIDENTIFIER NULL,
    [Player_PlayerId] UNIQUEIDENTIFIER NULL,
    [ForumTopicId]    INT              DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_dbo.AdminActions] PRIMARY KEY CLUSTERED ([AdminActionId] ASC),
    CONSTRAINT [FK_dbo.AdminActions_dbo.UserProfiles_Id] FOREIGN KEY ([UserProfileId]) REFERENCES [dbo].[UserProfiles] ([Id]),
    CONSTRAINT [FK_dbo.AdminActions_dbo.Player2_Player_PlayerId] FOREIGN KEY ([Player_PlayerId]) REFERENCES [dbo].[Players] ([PlayerId])
);


GO
CREATE NONCLUSTERED INDEX [IX_Player_PlayerId]
    ON [dbo].[AdminActions]([Player_PlayerId] ASC);
