CREATE TABLE [dbo].[RconMonitors] (
    [RconMonitorId]          UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [LastUpdated]            DATETIME         NOT NULL,
    [MonitorMapRotation]     BIT              NOT NULL,
    [MapRotationLastUpdated] DATETIME         NOT NULL,
    [LastError]              NVARCHAR (MAX)   NULL,
    [GameServer_ServerId]    UNIQUEIDENTIFIER NULL,
    [MonitorPlayerIPs]       BIT              DEFAULT ((0)) NOT NULL,
    [MonitorPlayers]         BIT              DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_dbo.RconMonitors] PRIMARY KEY CLUSTERED ([RconMonitorId] ASC),
    CONSTRAINT [FK_dbo.RconMonitors_dbo.GameServers_GameServer_ServerId] FOREIGN KEY ([GameServer_ServerId]) REFERENCES [dbo].[GameServers] ([ServerId])
);


GO
CREATE NONCLUSTERED INDEX [IX_GameServer_ServerId]
    ON [dbo].[RconMonitors]([GameServer_ServerId] ASC);

