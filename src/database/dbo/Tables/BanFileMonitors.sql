CREATE TABLE [dbo].[BanFileMonitors] (
    [BanFileMonitorId]  UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [GameServerId] UNIQUEIDENTIFIER NOT NULL,
    [FilePath]  NVARCHAR (MAX) NOT NULL,
    [RemoteFileSize] BIGINT NULL,
    [LastSync] DATETIME NULL,
    CONSTRAINT [PK_dbo.BanFileMonitors] PRIMARY KEY CLUSTERED ([BanFileMonitorId] ASC),
    CONSTRAINT [FK_dbo.BanFileMonitors_dbo.GameServers_GameServerId] FOREIGN KEY ([GameServerId]) REFERENCES [dbo].[GameServers] ([GameServerId])
);

GO
CREATE NONCLUSTERED INDEX [IX_GameServerId]
    ON [dbo].[BanFileMonitors]([GameServerId] ASC);

