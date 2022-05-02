CREATE TABLE [dbo].[MapFiles] (
    [MapFileId] UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [FileName]  NVARCHAR (MAX)   NULL,
    [Map_MapId] UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_dbo.MapFiles] PRIMARY KEY CLUSTERED ([MapFileId] ASC),
    CONSTRAINT [FK_dbo.MapFiles_dbo.Maps_Map_MapId] FOREIGN KEY ([Map_MapId]) REFERENCES [dbo].[Maps] ([MapId])
);


GO
CREATE NONCLUSTERED INDEX [IX_Map_MapId]
    ON [dbo].[MapFiles]([Map_MapId] ASC);

