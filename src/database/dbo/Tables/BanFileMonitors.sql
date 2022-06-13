CREATE TABLE [dbo].[BanFileMonitors] (
    [BanFileMonitorId]      UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [GameServerId]          UNIQUEIDENTIFIER NULL,
    [FilePath]              NVARCHAR (MAX)   NULL,
    [RemoteFileSize]        BIGINT           NOT NULL,
    [LastSync]              DATETIME         NOT NULL,
    [LastError]             NVARCHAR (MAX)   NULL,
    CONSTRAINT [PK_dbo.BanFileMonitors] PRIMARY KEY CLUSTERED ([BanFileMonitorId] ASC),
    CONSTRAINT [FK_dbo.BanFileMonitors_dbo.GameServers_GameServerId] FOREIGN KEY ([GameServerId]) REFERENCES [dbo].[GameServers] ([GameServerId])
);

GO
CREATE NONCLUSTERED INDEX [IX_GameServers_GameServerId]
    ON [dbo].[BanFileMonitors]([GameServerId] ASC);

