CREATE TABLE [dbo].[Players] (
    [PlayerId]  UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [GameType]  INT              NOT NULL,
    [Username]  NVARCHAR (MAX)   NULL,
    [Guid]      NVARCHAR (MAX)   NULL,
    [FirstSeen] DATETIME         NOT NULL,
    [LastSeen]  DATETIME         NOT NULL,
    [IpAddress] NVARCHAR (MAX)   NULL,
    CONSTRAINT [PK_dbo.Players] PRIMARY KEY CLUSTERED ([PlayerId] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_GameTypeAndLastSeen]
    ON [dbo].[Players]([GameType] ASC, [LastSeen] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_PlayerId]
    ON [dbo].[Players]([PlayerId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_GameType]
    ON [dbo].[Players]([GameType] ASC);

