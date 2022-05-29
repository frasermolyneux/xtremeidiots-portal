CREATE TABLE [dbo].[Demoes] (
    [DemoId]   UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [Game]     INT              NOT NULL,
    [Name]     NVARCHAR (MAX)   NULL,
    [FileName] NVARCHAR (MAX)   NULL,
    [Date]     DATETIME         NULL,
    [Map]      NVARCHAR (MAX)   NULL,
    [Mod]      NVARCHAR (MAX)   NULL,
    [GameType] NVARCHAR (MAX)   NULL,
    [Server]   NVARCHAR (MAX)   NULL,
    [Size]     BIGINT           NOT NULL,
    [UserProfileId] UNIQUEIDENTIFIER NULL,
    [DemoFileUri] NVARCHAR(MAX) NULL, 
    CONSTRAINT [PK_dbo.Demoes] PRIMARY KEY CLUSTERED ([DemoId] ASC),
    CONSTRAINT [FK_dbo.Demoes_dbo.UserProfiles_Id] FOREIGN KEY ([UserProfileId]) REFERENCES [dbo].[UserProfiles] ([Id])
);
