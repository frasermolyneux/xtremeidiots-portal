CREATE TABLE [dbo].[LivePlayers] (
    [LivePlayerId]        UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [Name]                NVARCHAR (60)   NULL,
    [Score]               INT              NOT NULL,
    [Ping]                INT              NOT NULL,
    [Team]                NVARCHAR (10)   NULL,
    [Time]                TIME (7)         NOT NULL,
    [IpAddress]            NVARCHAR (60)   NULL,
    [Lat]                 FLOAT (53)       NULL,
    [Long]                FLOAT (53)       NULL,
    [CountryCode]         NVARCHAR (60)   NULL,
    [GameServer_ServerId] UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_dbo.LivePlayers] PRIMARY KEY CLUSTERED ([LivePlayerId] ASC),
    CONSTRAINT [FK_dbo.LivePlayers_dbo.GameServers_GameServer_ServerId] FOREIGN KEY ([GameServer_ServerId]) REFERENCES [dbo].[GameServers] ([ServerId])
);


GO
CREATE NONCLUSTERED INDEX [IX_GameServer_ServerId]
    ON [dbo].[LivePlayers]([GameServer_ServerId] ASC);

