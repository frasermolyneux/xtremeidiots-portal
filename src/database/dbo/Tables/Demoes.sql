CREATE TABLE [dbo].[Demoes] (
    [DemoId]   UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [Game]     INT              NOT NULL,
    [Name]     NVARCHAR (MAX)   NULL,
    [FileName] NVARCHAR (MAX)   NULL,
    [Date]     DATETIME         NOT NULL,
    [Map]      NVARCHAR (MAX)   NULL,
    [Mod]      NVARCHAR (MAX)   NULL,
    [GameType] NVARCHAR (MAX)   NULL,
    [Server]   NVARCHAR (MAX)   NULL,
    [Size]     BIGINT           NOT NULL,
    [User_Id]  NVARCHAR (128)   NULL,
    [DemoFileUri] NVARCHAR(MAX) NULL, 
    CONSTRAINT [PK_dbo.Demoes] PRIMARY KEY CLUSTERED ([DemoId] ASC),
    CONSTRAINT [FK_dbo.Demoes_dbo.AspNetUsers_User_Id] FOREIGN KEY ([User_Id]) REFERENCES [dbo].[AspNetUsers] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_User_Id]
    ON [dbo].[Demoes]([User_Id] ASC);

