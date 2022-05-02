CREATE TABLE [dbo].[PlayerAlias] (
    [PlayerAliasId]   UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [Name]            NVARCHAR (60)    NULL,
    [Added]           DATETIME         NOT NULL,
    [LastUsed]        DATETIME         NOT NULL,
    [Player_PlayerId] UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_dbo.PlayerAlias] PRIMARY KEY CLUSTERED ([PlayerAliasId] ASC),
    CONSTRAINT [FK_dbo.PlayerAlias_dbo.Player2_Player_PlayerId] FOREIGN KEY ([Player_PlayerId]) REFERENCES [dbo].[Player2] ([PlayerId])
);


GO
CREATE NONCLUSTERED INDEX [IX_Name]
    ON [dbo].[PlayerAlias]([Name] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Player_PlayerId]
    ON [dbo].[PlayerAlias]([Player_PlayerId] ASC);

