CREATE FULLTEXT INDEX ON [dbo].[ChatMessages]
    ([Username] LANGUAGE 1033, [Message] LANGUAGE 1033)
    KEY INDEX [PK_dbo.ChatMessage]
    ON [fts_chatlogs];

