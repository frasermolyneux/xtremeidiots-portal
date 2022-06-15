CREATE TABLE [dbo].[GameServerEvents] (
	[GameServerEventId] UNIQUEIDENTIFIER NOT NULL  DEFAULT newsequentialid(), 
    [GameServerId] UNIQUEIDENTIFIER NOT NULL, 
    [Timestamp] DATETIME NOT NULL, 
    [EventType] VARCHAR(50) NOT NULL, 
    [EventData] VARCHAR(MAX) NULL, 
    CONSTRAINT [PK_GameServerEvents] PRIMARY KEY ([GameServerEventId]),
    CONSTRAINT [FK_GameServerEvents_GameServerId] FOREIGN KEY ([GameServerId]) REFERENCES [dbo].[GameServers] ([GameServerId])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_GameServerEventId]
    ON [dbo].[GameServerEvents]([GameServerEventId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_GameServerId]
    ON [dbo].[GameServerEvents]([GameServerId] ASC);