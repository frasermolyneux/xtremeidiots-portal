CREATE TABLE [dbo].[PlayerIpAddresses] (
    [PlayerIpAddressId] UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
    [Address]           NVARCHAR (60)    NULL,
    [Added]             DATETIME         NOT NULL,
    [LastUsed]          DATETIME         NOT NULL,
    [Player_PlayerId]   UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_dbo.PlayerIpAddresses] PRIMARY KEY CLUSTERED ([PlayerIpAddressId] ASC),
    CONSTRAINT [FK_dbo.PlayerIpAddresses_dbo.Player2_Player_PlayerId] FOREIGN KEY ([Player_PlayerId]) REFERENCES [dbo].[Player2] ([PlayerId])
);


GO
CREATE NONCLUSTERED INDEX [IX_Player_PlayerId]
    ON [dbo].[PlayerIpAddresses]([Player_PlayerId] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Address]
    ON [dbo].[PlayerIpAddresses]([Address] ASC);

