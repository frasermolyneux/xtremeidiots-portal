CREATE TABLE [dbo].[MapRotations] (
    [MapRotationId]       UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [GameMode]            NVARCHAR (MAX)   NULL,
    [GameServer_ServerId] UNIQUEIDENTIFIER NULL,
    [Map_MapId]           UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_dbo.MapRotations] PRIMARY KEY CLUSTERED ([MapRotationId] ASC),
    CONSTRAINT [FK_dbo.MapRotations_dbo.GameServers_GameServer_ServerId] FOREIGN KEY ([GameServer_ServerId]) REFERENCES [dbo].[GameServers] ([ServerId]),
    CONSTRAINT [FK_dbo.MapRotations_dbo.Maps_Map_MapId] FOREIGN KEY ([Map_MapId]) REFERENCES [dbo].[Maps] ([MapId])
);


GO
CREATE NONCLUSTERED INDEX [IX_GameServer_ServerId]
    ON [dbo].[MapRotations]([GameServer_ServerId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Map_MapId]
    ON [dbo].[MapRotations]([Map_MapId] ASC);

