CREATE TABLE [dbo].[PlayerIpAddresses] (
    [PlayerIpAddressId] UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [PlayerId] UNIQUEIDENTIFIER NULL,
    [Address] NVARCHAR (60) NULL,
    [Added] DATETIME NOT NULL,
    [LastUsed] DATETIME NOT NULL,
    [ConfidenceScore] INT NOT NULL DEFAULT 0, 
    CONSTRAINT [PK_dbo.PlayerIpAddresses] PRIMARY KEY CLUSTERED ([PlayerIpAddressId] ASC),
    CONSTRAINT [FK_dbo.PlayerIpAddresses_dbo.Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [dbo].[Players] ([PlayerId])
);

GO
CREATE NONCLUSTERED INDEX [IX_Players_PlayerId]
    ON [dbo].[PlayerIpAddresses]([PlayerId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Address]
    ON [dbo].[PlayerIpAddresses]([Address] ASC);
