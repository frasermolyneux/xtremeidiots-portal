CREATE TABLE [dbo].[GameServerStats]
(
	[GameServerStatId] UNIQUEIDENTIFIER NOT NULL DEFAULT newsequentialid(),
	[GameServerId] UNIQUEIDENTIFIER NULL, 
	[PlayerCount] INT NOT NULL,
	[MapName] NVARCHAR(MAX) NOT NULL,
	[Timestamp] DATETIME NOT NULL,
    CONSTRAINT [FK_GameServerStats_GameServer] FOREIGN KEY ([GameServerId]) REFERENCES [dbo].[GameServers] ([GameServerId]), 
    CONSTRAINT [PK_GameServerStats] PRIMARY KEY ([GameServerStatId])
)
