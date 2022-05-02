CREATE TABLE [dbo].[BanFileMonitors] (
    [BanFileMonitorId]    UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [FilePath]            NVARCHAR (MAX)   NULL,
    [RemoteFileSize]      BIGINT           NOT NULL,
    [LastSync]            DATETIME         NOT NULL,
    [LastError]           NVARCHAR (MAX)   NULL,
    [GameServer_ServerId] UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_dbo.BanFileMonitors] PRIMARY KEY CLUSTERED ([BanFileMonitorId] ASC),
    CONSTRAINT [FK_dbo.BanFileMonitors_dbo.GameServers_GameServer_ServerId] FOREIGN KEY ([GameServer_ServerId]) REFERENCES [dbo].[GameServers] ([ServerId])
);


GO
CREATE NONCLUSTERED INDEX [IX_GameServer_ServerId]
    ON [dbo].[BanFileMonitors]([GameServer_ServerId] ASC);

