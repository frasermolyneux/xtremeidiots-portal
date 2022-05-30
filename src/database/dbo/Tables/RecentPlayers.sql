CREATE TABLE [dbo].[RecentPlayers]
(
    [Id] UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [Name] NVARCHAR (60) NOT NULL,
    [IpAddress] NVARCHAR (60) NOT NULL,
    [Lat] FLOAT (53) NOT NULL,
    [Long] FLOAT (53) NOT NULL,
    [CountryCode] NVARCHAR (60) NOT NULL,
    [GameType] INT DEFAULT 0 NOT NULL,
    [PlayerId] UNIQUEIDENTIFIER NULL,
    [ServerId] UNIQUEIDENTIFIER NULL,
    [Timestamp] DATETIME NOT NULL,
    CONSTRAINT [PK_dbo.RecentPlayers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.RecentPlayers_dbo.GameServers_ServerId] FOREIGN KEY ([ServerId]) REFERENCES [dbo].[GameServers] ([ServerId]),
    CONSTRAINT [FK_dbo.RecentPlayers_dbo.Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Players] ([PlayerId])
)

GO
CREATE NONCLUSTERED INDEX [IX_GameServer_ServerId]
    ON [dbo].[RecentPlayers]([ServerId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Players_PlayerId]
    ON [dbo].[RecentPlayers]([PlayerId] ASC);