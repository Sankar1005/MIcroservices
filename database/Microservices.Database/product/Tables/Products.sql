CREATE TABLE [product].[Products]
(
    [Id]            INT IDENTITY (1, 1) NOT NULL,
    [Name]          NVARCHAR (200)      NOT NULL,
    [Description]   NVARCHAR (1000)     NULL,
    [Price]         DECIMAL (18, 2)     NOT NULL,
    [StockQuantity] INT                 NOT NULL CONSTRAINT [DF_Products_StockQuantity] DEFAULT ((0)),
    [IsActive]      BIT                 NOT NULL CONSTRAINT [DF_Products_IsActive] DEFAULT ((1)),
    [CreatedAt]     DATETIME2 (7)       NOT NULL CONSTRAINT [DF_Products_CreatedAt] DEFAULT (sysutcdatetime()),
    [UpdatedAt]     DATETIME2 (7)       NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [CK_Products_Price] CHECK ([Price] > (0)),
    CONSTRAINT [CK_Products_StockQuantity] CHECK ([StockQuantity] >= (0))
);
