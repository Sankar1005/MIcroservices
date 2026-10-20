CREATE TABLE [ordering].[Orders]
(
    [Id]           INT IDENTITY (1, 1) NOT NULL,
    [CustomerName] NVARCHAR (200)      NOT NULL,
    [TotalAmount]  DECIMAL (18, 2)     NOT NULL,
    [IsActive]     BIT                 NOT NULL CONSTRAINT [DF_Orders_IsActive] DEFAULT ((1)),
    [CreatedAt]    DATETIME2 (7)       NOT NULL CONSTRAINT [DF_Orders_CreatedAt] DEFAULT (sysutcdatetime()),
    [UpdatedAt]    DATETIME2 (7)       NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_Orders_TotalAmount] CHECK ([TotalAmount] >= (0))
);
