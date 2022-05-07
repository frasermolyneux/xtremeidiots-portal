CREATE TABLE [dbo].[Maps] (
    [MapId]    UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [GameType] INT              NOT NULL,
    [MapName]  NVARCHAR (MAX)   NOT NULL,
    [MapFiles] NVARCHAR(MAX) NULL , 
    [MapPopularity] NVARCHAR(MAX) NULL , 
    CONSTRAINT [PK_dbo.Maps] PRIMARY KEY CLUSTERED ([MapId] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_MapId]
    ON [dbo].[Maps]([MapId] ASC);

