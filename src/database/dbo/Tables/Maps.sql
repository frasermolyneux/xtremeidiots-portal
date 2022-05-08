CREATE TABLE [dbo].[Maps] (
    [MapId]    UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [GameType] INT              NOT NULL,
    [MapName]  NVARCHAR (MAX)   NULL,
    [MapFiles] NVARCHAR(MAX) NULL , 
    [TotalLikes] INT NOT NULL DEFAULT 0, 
    [TotalDislikes] INT NOT NULL DEFAULT 0, 
    [TotalVotes] INT NOT NULL DEFAULT 0, 
    [LikePercentage] FLOAT NOT NULL DEFAULT 0, 
    [DislikePercentage] FLOAT NOT NULL DEFAULT 0, 
    CONSTRAINT [PK_dbo.Maps] PRIMARY KEY CLUSTERED ([MapId] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_MapId]
    ON [dbo].[Maps]([MapId] ASC);

