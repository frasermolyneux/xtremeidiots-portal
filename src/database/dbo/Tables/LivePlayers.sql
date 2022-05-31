CREATE TABLE [dbo].[LivePlayers] (
    [Id] UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
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
    [PlayerId] UNIQUEIDENTIFIER NULL,
    [GameServer_ServerId] UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_dbo.LivePlayers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_dbo.LivePlayers_dbo.GameServers_GameServer_ServerId] FOREIGN KEY ([GameServer_ServerId]) REFERENCES [dbo].[GameServers] ([ServerId]),
    CONSTRAINT [FK_dbo.LivePlayers_dbo.Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Players] ([PlayerId])
);

GO
CREATE NONCLUSTERED INDEX [IX_GameServer_ServerId]
    ON [dbo].[LivePlayers]([GameServer_ServerId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Players_PlayerId]
    ON [dbo].[LivePlayers]([PlayerId] ASC);
