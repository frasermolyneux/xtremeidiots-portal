CREATE TABLE [dbo].[LivePlayers] (
    [LivePlayerId]        UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [Name]                NVARCHAR (MAX)   NULL,
    [Score]               INT              NOT NULL,
    [Ping]                INT              NOT NULL,
    [Team]                NVARCHAR (MAX)   NULL,
    [Time]                TIME (7)         NOT NULL,
    [GameServer_ServerId] UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_dbo.LivePlayers] PRIMARY KEY CLUSTERED ([LivePlayerId] ASC),
    CONSTRAINT [FK_dbo.LivePlayers_dbo.GameServers_GameServer_ServerId] FOREIGN KEY ([GameServer_ServerId]) REFERENCES [dbo].[GameServers] ([ServerId])
);


GO
CREATE NONCLUSTERED INDEX [IX_GameServer_ServerId]
    ON [dbo].[LivePlayers]([GameServer_ServerId] ASC);

