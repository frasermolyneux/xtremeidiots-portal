CREATE TABLE [dbo].[MapVotes] (
    [MapVoteId] UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [MapId] UNIQUEIDENTIFIER NOT NULL,
    [PlayerId] UNIQUEIDENTIFIER NOT NULL,
    [GameServerId] UNIQUEIDENTIFIER NULL, 
    [Like] BIT NOT NULL,
    [Timestamp] DATETIME NOT NULL,
    CONSTRAINT [PK_dbo.MapVotes] PRIMARY KEY CLUSTERED ([MapVoteId] ASC),
    CONSTRAINT [FK_dbo.MapVotes_dbo.Maps_MapId] FOREIGN KEY ([MapId]) REFERENCES [dbo].[Maps] ([MapId]),
    CONSTRAINT [FK_dbo.MapVotes_dbo.Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Players] ([PlayerId]),
    CONSTRAINT [FK_dbo.MapVotes_dbo.GameServers_GameServerId] FOREIGN KEY ([GameServerId]) REFERENCES [dbo].[GameServers] ([GameServerId]), 
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_MapVoteId]
    ON [dbo].[MapVotes]([MapVoteId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_MapId]
    ON [dbo].[MapVotes]([MapId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_PlayerId]
    ON [dbo].[MapVotes]([PlayerId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_GameServerId]
    ON [dbo].[MapVotes]([GameServerId] ASC);
