CREATE TABLE [dbo].[GameServerStats]
(
	[Id] UNIQUEIDENTIFIER NOT NULL DEFAULT newsequentialid(),
	[GameServerId] UNIQUEIDENTIFIER NULL, 
	[PlayerCount] INT NOT NULL,
	[MapName] NVARCHAR(MAX) NOT NULL,
	[Timestamp] DATETIME NOT NULL,
    CONSTRAINT [FK_GameServerStats_GameServer] FOREIGN KEY ([GameServerId]) REFERENCES [dbo].[GameServers] ([ServerId]), 
    CONSTRAINT [PK_GameServerStats] PRIMARY KEY ([Id])
)
