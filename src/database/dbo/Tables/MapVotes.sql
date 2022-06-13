CREATE TABLE [dbo].[MapVotes] (
    [MapVoteId]       UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [MapId]       UNIQUEIDENTIFIER NULL,
    [PlayerId] UNIQUEIDENTIFIER NULL,
    [Like]            BIT              NOT NULL,
    [Timestamp]       DATETIME         NOT NULL,
    CONSTRAINT [PK_dbo.MapVotes] PRIMARY KEY CLUSTERED ([MapVoteId] ASC),
    CONSTRAINT [FK_dbo.MapVotes_dbo.Maps_Map_MapId] FOREIGN KEY ([MapId]) REFERENCES [dbo].[Maps] ([MapId]),
    CONSTRAINT [FK_dbo.MapVotes_dbo.Player2_Player_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Players] ([PlayerId])
);


GO
CREATE NONCLUSTERED INDEX [IX_Players_PlayerId]
    ON [dbo].[MapVotes]([PlayerId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Maps_MapId]
    ON [dbo].[MapVotes]([MapId] ASC);

