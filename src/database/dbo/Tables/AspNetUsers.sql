CREATE TABLE [dbo].[AspNetUsers] (
    [Id]                                      NVARCHAR (128) NOT NULL,
    [XtremeIdiotsId]                          NVARCHAR (MAX) NULL,
    [XtremeIdiotsTitle]                       NVARCHAR (MAX) NULL,
    [XtremeIdiotsFormattedName]               NVARCHAR (MAX) NULL,
    [XtremeIdiotsPrimaryGroupId]              NVARCHAR (MAX) NULL,
    [XtremeIdiotsPrimaryGroupName]            NVARCHAR (MAX) NULL,
    [XtremeIdiotsPrimaryGroupIdFormattedName] NVARCHAR (MAX) NULL,
    [XtremeIdiotsPhotoUrl]                    NVARCHAR (MAX) NULL,
    [XtremeIdiotsPhotoUrlIsDefault]           NVARCHAR (MAX) NULL,
    [Email]                                   NVARCHAR (256) NULL,
    [EmailConfirmed]                          BIT            NOT NULL,
    [PasswordHash]                            NVARCHAR (MAX) NULL,
    [SecurityStamp]                           NVARCHAR (MAX) NULL,
    [PhoneNumber]                             NVARCHAR (MAX) NULL,
    [PhoneNumberConfirmed]                    BIT            NOT NULL,
    [TwoFactorEnabled]                        BIT            NOT NULL,
    [LockoutEndDateUtc]                       DATETIME       NULL,
    [LockoutEnabled]                          BIT            NOT NULL,
    [AccessFailedCount]                       INT            NOT NULL,
    [UserName]                                NVARCHAR (256) NOT NULL,
    [DemoManagerAuthKey]                      NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex]
    ON [dbo].[AspNetUsers]([UserName] ASC);

