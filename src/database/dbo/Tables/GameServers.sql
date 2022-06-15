CREATE TABLE [dbo].[GameServers] (
    [GameServerId] UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [Title] NVARCHAR (60) NOT NULL,
    [GameType] INT DEFAULT 0 NOT NULL,
    [Hostname] NVARCHAR (MAX) NOT NULL,
    [QueryPort] INT DEFAULT 0 NOT NULL,
    [FtpHostname] NVARCHAR (MAX) NULL,
    [FtpPort] INT DEFAULT 21 NULL,
    [FtpUsername] NVARCHAR (MAX) NULL,
    [FtpPassword] NVARCHAR (MAX) NULL,
    [RconPassword] NVARCHAR (MAX) NULL,
    [ServerListPosition] INT DEFAULT 0 NOT NULL,
    [HtmlBanner] NVARCHAR (MAX) NULL,
    [BannerServerListEnabled] BIT DEFAULT 0 NOT NULL,
    [PortalServerListEnabled] BIT DEFAULT 0 NOT NULL,
    [ChatLogEnabled] BIT DEFAULT 0 NOT NULL,
    [LiveTrackingEnabled] BIT NOT NULL DEFAULT 0, 
    [LiveTitle] NVARCHAR (MAX) NULL,
    [LiveMap] NVARCHAR (MAX) NULL,
    [LiveMod] NVARCHAR (MAX) NULL,
    [LiveMaxPlayers] INT DEFAULT 0 NULL,
    [LiveCurrentPlayers] INT DEFAULT 0 NULL,
    [LiveLastUpdated] DATETIME DEFAULT ('1900-01-01T00:00:00.000') NULL,
    CONSTRAINT [PK_dbo.GameServers] PRIMARY KEY CLUSTERED ([GameServerId] ASC)
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_GameServerId]
    ON [dbo].[GameServers]([GameServerId] ASC);
