CREATE TABLE [dbo].[RecentPlayers] (
    [RecentPlayerId] UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [PlayerId] UNIQUEIDENTIFIER NULL,
    [GameServerId] UNIQUEIDENTIFIER NULL,
    [Name] NVARCHAR (60) NOT NULL,
    [IpAddress] NVARCHAR (60) NULL,
    [Lat] FLOAT (53) NULL,
    [Long] FLOAT (53) NULL,
    [CountryCode] NVARCHAR (60) NULL,
    [GameType] INT DEFAULT 0 NOT NULL,
    [Timestamp] DATETIME NOT NULL,
    CONSTRAINT [PK_dbo.RecentPlayers] PRIMARY KEY CLUSTERED ([RecentPlayerId] ASC),
    CONSTRAINT [FK_dbo.RecentPlayers_dbo.GameServers_GameServerId] FOREIGN KEY ([GameServerId]) REFERENCES [dbo].[GameServers] ([GameServerId]),
    CONSTRAINT [FK_dbo.RecentPlayers_dbo.Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Players] ([PlayerId])
)

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_RecentPlayerId]
    ON [dbo].[RecentPlayers]([RecentPlayerId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_GameServerId]
    ON [dbo].[RecentPlayers]([GameServerId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_PlayerId]
    ON [dbo].[RecentPlayers]([PlayerId] ASC);