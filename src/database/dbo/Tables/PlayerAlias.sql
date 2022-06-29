CREATE TABLE [dbo].[PlayerAlias] (
    [PlayerAliasId] UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [PlayerId] UNIQUEIDENTIFIER NULL,
    [Name] NVARCHAR (60) NULL,
    [Added] DATETIME NOT NULL,
    [LastUsed] DATETIME NOT NULL,
    CONSTRAINT [PK_dbo.PlayerAlias] PRIMARY KEY CLUSTERED ([PlayerAliasId] ASC),
    CONSTRAINT [FK_dbo.PlayerAlias_dbo.Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Players] ([PlayerId])
);

GO
CREATE NONCLUSTERED INDEX [IX_PlayerId]
    ON [dbo].[PlayerAlias]([PlayerId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Name]
    ON [dbo].[PlayerAlias]([Name] ASC);
