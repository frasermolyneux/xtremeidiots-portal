CREATE TABLE [dbo].[GameServers] (
    [GameServerId]                 UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [Title]                    NVARCHAR (60)    NULL,
    [HtmlBanner]               NVARCHAR (MAX)   NULL,
    [GameType]                 INT              DEFAULT 0 NOT NULL,
    [Hostname]                 NVARCHAR (MAX)   NULL,
    [QueryPort]                INT              DEFAULT 0 NOT NULL,
    [FtpHostname]              NVARCHAR (MAX)   NULL,
    [FtpPort]                INT              DEFAULT 21 NOT NULL,
    [FtpUsername]              NVARCHAR (MAX)   NULL,
    [FtpPassword]              NVARCHAR (MAX)   NULL,
    [LiveStatusEnabled] BIT NOT NULL DEFAULT 0, 
    [LiveTitle]                NVARCHAR (MAX)   NULL,
    [LiveMap]                  NVARCHAR (MAX)   NULL,
    [LiveMod]                  NVARCHAR (MAX)   NULL,
    [LiveMaxPlayers]           INT              DEFAULT 0 NOT NULL,
    [LiveCurrentPlayers]       INT              DEFAULT 0 NOT NULL,
    [LiveLastUpdated]          DATETIME         DEFAULT ('1900-01-01T00:00:00.000') NOT NULL,
    [ShowOnBannerServerList]   BIT              DEFAULT 0 NOT NULL,
    [BannerServerListPosition] INT              DEFAULT 0 NOT NULL,
    [ShowOnPortalServerList]   BIT              DEFAULT 0 NOT NULL,
    [ShowChatLog]              BIT              DEFAULT 0 NOT NULL,
    [RconPassword]             NVARCHAR (MAX)   NULL ,
    CONSTRAINT [PK_dbo.GameServers] PRIMARY KEY CLUSTERED ([GameServerId] ASC)
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_GameServerId]
    ON [dbo].[GameServers]([GameServerId] ASC);

