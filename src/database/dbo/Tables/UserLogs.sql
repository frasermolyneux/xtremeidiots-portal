CREATE TABLE [dbo].[UserLogs] (
    [UserLogId]          UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [Message]            NVARCHAR (MAX)   NULL,
    [Timestamp]          DATETIME         NOT NULL,
    [ApplicationUser_Id] NVARCHAR (128)   NULL,
    CONSTRAINT [PK_dbo.UserLogs] PRIMARY KEY CLUSTERED ([UserLogId] ASC),
    CONSTRAINT [FK_dbo.UserLogs_dbo.AspNetUsers_ApplicationUser_Id] FOREIGN KEY ([ApplicationUser_Id]) REFERENCES [dbo].[AspNetUsers] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_ApplicationUser_Id]
    ON [dbo].[UserLogs]([ApplicationUser_Id] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Timestamp]
    ON [dbo].[UserLogs]([Timestamp] ASC);

