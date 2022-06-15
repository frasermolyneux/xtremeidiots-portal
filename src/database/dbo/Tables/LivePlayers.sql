CREATE TABLE [dbo].[LivePlayers] (
    [LivePlayerId] UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [PlayerId] UNIQUEIDENTIFIER NULL,
    [GameServerId] UNIQUEIDENTIFIER NULL,
    [Name] NVARCHAR (60) NOT NULL,
    [Score] INT NOT NULL DEFAULT 0,
    [Ping] INT NOT NULL DEFAULT 0,
    [Num] INT NOT NULL DEFAULT 0,
    [Rate] INT NOT NULL DEFAULT 0,
    [Team] NVARCHAR (10) NULL,
    [Time] TIME (7) NOT NULL,
    [IpAddress] NVARCHAR (60) NULL,
    [Lat] FLOAT (53) NULL,
    [Long] FLOAT (53) NULL,
    [CountryCode] NVARCHAR (60) NULL,
    [GameType] INT DEFAULT 0 NOT NULL,
    CONSTRAINT [PK_dbo.LivePlayers] PRIMARY KEY CLUSTERED ([LivePlayerId] ASC),
    CONSTRAINT [FK_dbo.LivePlayers_dbo.GameServers_GameServer_GameServerId] FOREIGN KEY ([GameServerId]) REFERENCES [dbo].[GameServers] ([GameServerId]),
    CONSTRAINT [FK_dbo.LivePlayers_dbo.Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Players] ([PlayerId])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_LivePlayerId]
    ON [dbo].[LivePlayers]([LivePlayerId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_GameServer_GameServerId]
    ON [dbo].[LivePlayers]([GameServerId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Players_PlayerId]
    ON [dbo].[LivePlayers]([PlayerId] ASC);
