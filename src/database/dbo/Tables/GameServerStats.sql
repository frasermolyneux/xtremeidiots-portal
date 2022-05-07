CREATE TABLE [dbo].[GameServerStats]
(
	[Id] INT NOT NULL PRIMARY KEY,
	[GameServerId] UNIQUEIDENTIFIER NULL, 
	[PlayerCount] INT NOT NULL,
	[MapName] NVARCHAR(MAX) NOT NULL,
	[Timestamp] DATETIME NOT NULL,
    CONSTRAINT [FK_GameServerStats_GameServer] FOREIGN KEY ([GameServerId]) REFERENCES [dbo].[GameServers] ([ServerId])
)
