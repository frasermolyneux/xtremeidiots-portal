CREATE TABLE [dbo].[LivePlayerLocations] (
    [LivePlayerLocationId] UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [IpAddress]            NVARCHAR (MAX)   NULL,
    [Lat]                  FLOAT (53)       NOT NULL,
    [Long]                 FLOAT (53)       NOT NULL,
    [LastSeen]             DATETIME         NOT NULL,
    CONSTRAINT [PK_dbo.LivePlayerLocations] PRIMARY KEY CLUSTERED ([LivePlayerLocationId] ASC)
);

