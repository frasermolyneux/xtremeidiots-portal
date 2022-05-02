CREATE TABLE [dbo].[FileMonitors] (
    [FileMonitorId]       UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [FilePath]            NVARCHAR (MAX)   NULL,
    [BytesRead]           BIGINT           NOT NULL,
    [LastRead]            DATETIME         NOT NULL,
    [LastError]           NVARCHAR (MAX)   NULL,
    [GameServer_ServerId] UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_dbo.FileMonitors] PRIMARY KEY CLUSTERED ([FileMonitorId] ASC),
    CONSTRAINT [FK_dbo.FileMonitors_dbo.GameServers_GameServer_ServerId] FOREIGN KEY ([GameServer_ServerId]) REFERENCES [dbo].[GameServers] ([ServerId])
);


GO
CREATE NONCLUSTERED INDEX [IX_GameServer_ServerId]
    ON [dbo].[FileMonitors]([GameServer_ServerId] ASC);

