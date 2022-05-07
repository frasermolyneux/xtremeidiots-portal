CREATE TABLE [dbo].[GameServerEvents]
(
	[Id] UNIQUEIDENTIFIER NOT NULL  DEFAULT newsequentialid(), 
    [GameServerId] UNIQUEIDENTIFIER NOT NULL, 
    [Timestamp] DATETIME NOT NULL, 
    [EventType] VARCHAR(50) NOT NULL, 
    [EventData] VARCHAR(MAX) NULL, 
    CONSTRAINT [PK_GameServerEvents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GameServerEvents_GameServer] FOREIGN KEY ([GameServerId]) REFERENCES [dbo].[GameServers] ([ServerId])
) 