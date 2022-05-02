CREATE FULLTEXT INDEX ON [dbo].[ChatLogs]
    ([Username] LANGUAGE 1033, [Message] LANGUAGE 1033)
    KEY INDEX [PK_dbo.ChatLogs]
    ON [fts_chatlogs];

