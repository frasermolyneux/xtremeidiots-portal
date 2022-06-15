CREATE TABLE [dbo].[MapVotes] (
    [MapVoteId] UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [MapId] UNIQUEIDENTIFIER NULL,
    [PlayerId] UNIQUEIDENTIFIER NULL,
    [Like] BIT NOT NULL,
    [Timestamp] DATETIME NOT NULL,
    CONSTRAINT [PK_dbo.MapVotes] PRIMARY KEY CLUSTERED ([MapVoteId] ASC),
    CONSTRAINT [FK_dbo.MapVotes_dbo.Maps_MapId] FOREIGN KEY ([MapId]) REFERENCES [dbo].[Maps] ([MapId]),
    CONSTRAINT [FK_dbo.MapVotes_dbo.Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Players] ([PlayerId])
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
