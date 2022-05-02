CREATE TABLE [dbo].[MapVotes] (
    [MapVoteId]       UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [Like]            BIT              NOT NULL,
    [Timestamp]       DATETIME         NOT NULL,
    [Map_MapId]       UNIQUEIDENTIFIER NULL,
    [Player_PlayerId] UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_dbo.MapVotes] PRIMARY KEY CLUSTERED ([MapVoteId] ASC),
    CONSTRAINT [FK_dbo.MapVotes_dbo.Maps_Map_MapId] FOREIGN KEY ([Map_MapId]) REFERENCES [dbo].[Maps] ([MapId]),
    CONSTRAINT [FK_dbo.MapVotes_dbo.Player2_Player_PlayerId] FOREIGN KEY ([Player_PlayerId]) REFERENCES [dbo].[Player2] ([PlayerId])
);


GO
CREATE NONCLUSTERED INDEX [IX_Player_PlayerId]
    ON [dbo].[MapVotes]([Player_PlayerId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Map_MapId]
    ON [dbo].[MapVotes]([Map_MapId] ASC);

