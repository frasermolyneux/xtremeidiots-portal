CREATE TABLE [dbo].[Demos] (
    [DemoId] UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [UserProfileId] UNIQUEIDENTIFIER NULL,
    [GameType] INT NOT NULL,
    [Title] NVARCHAR (MAX) NULL,
    [FileName] NVARCHAR (MAX) NULL,
    [Created] DATETIME NULL,
    [Map] NVARCHAR (MAX) NULL,
    [Mod] NVARCHAR (MAX) NULL,
    [GameMode] NVARCHAR (MAX) NULL,
    [ServerName] NVARCHAR (MAX) NULL,
    [FileSize] BIGINT NOT NULL,
    [FileUri] NVARCHAR(MAX) NULL, 
    CONSTRAINT [PK_dbo.Demos] PRIMARY KEY CLUSTERED ([DemoId] ASC),
    CONSTRAINT [FK_dbo.Demos_dbo.UserProfiles_UserProfileId] FOREIGN KEY ([UserProfileId]) REFERENCES [dbo].[UserProfiles] ([UserProfileId])
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_DemoId]
    ON [dbo].[Demos]([DemoId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_UserProfileId]
    ON [dbo].[Demos]([UserProfileId] ASC);
