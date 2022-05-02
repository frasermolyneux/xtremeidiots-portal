CREATE TABLE [dbo].[SystemLogs] (
    [SystemLogId] UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [Level]       NVARCHAR (MAX)   NULL,
    [Message]     NVARCHAR (MAX)   NULL,
    [Error]       NVARCHAR (MAX)   NULL,
    [Timestamp]   DATETIME         NOT NULL,
    CONSTRAINT [PK_dbo.SystemLogs] PRIMARY KEY CLUSTERED ([SystemLogId] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_Timestamp]
    ON [dbo].[SystemLogs]([Timestamp] ASC);

