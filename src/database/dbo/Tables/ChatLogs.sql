CREATE TABLE [dbo].[ChatLogs] (
    [ChatLogId]           UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [Username]            NVARCHAR (MAX)   NULL,
    [ChatType]            INT              NOT NULL,
    [Message]             NVARCHAR (MAX)   NULL,
    [Timestamp]           DATETIME         NOT NULL,
    [GameServer_ServerId] UNIQUEIDENTIFIER NULL,
    [Player_PlayerId]     UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_dbo.ChatLogs] PRIMARY KEY CLUSTERED ([ChatLogId] ASC),
    CONSTRAINT [FK_dbo.ChatLogs_dbo.GameServers_GameServer_ServerId] FOREIGN KEY ([GameServer_ServerId]) REFERENCES [dbo].[GameServers] ([ServerId]),
    CONSTRAINT [FK_dbo.ChatLogs_dbo.Player2_Player_PlayerId] FOREIGN KEY ([Player_PlayerId]) REFERENCES [dbo].[Players] ([PlayerId])
);


GO
CREATE NONCLUSTERED INDEX [IX_GameServer_ServerId]
    ON [dbo].[ChatLogs]([GameServer_ServerId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Timestamp]
    ON [dbo].[ChatLogs]([Timestamp] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Player_PlayerId]
    ON [dbo].[ChatLogs]([Player_PlayerId] ASC);

