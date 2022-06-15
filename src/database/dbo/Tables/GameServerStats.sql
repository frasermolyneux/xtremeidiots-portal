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

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_GameServerStatId]
    ON [dbo].[GameServerStats]([GameServerStatId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_GameServerId]
    ON [dbo].[GameServerStats]([GameServerId] ASC);
