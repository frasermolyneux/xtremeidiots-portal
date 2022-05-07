CREATE TABLE [dbo].[GameServerMaps]
(
	[Id] INT NOT NULL PRIMARY KEY,
	[GameServerId] UNIQUEIDENTIFIER NULL, 
	[MapId]       UNIQUEIDENTIFIER NULL,
	[Timestamp] DATETIME NOT NULL,
	CONSTRAINT [FK_GameServerMaps_Map] FOREIGN KEY ([MapId]) REFERENCES [dbo].[Maps] ([MapId]),
	CONSTRAINT [FK_GameServerMaps_GameServer] FOREIGN KEY ([GameServerId]) REFERENCES [dbo].[GameServers] ([ServerId])
)
