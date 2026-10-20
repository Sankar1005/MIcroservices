CREATE TABLE [ordering].[OrderItems]
(
    [Id]          INT IDENTITY (1, 1) NOT NULL,
    [OrderId]     INT                 NOT NULL,
    -- No foreign key to product.Products: products belong to ProductService (another service, its own schema).
    -- Name and price are copied here when the order is placed, so the order never changes
    -- if the product is renamed, repriced or deleted later.
    [ProductId]   INT                 NOT NULL,
    [ProductName] NVARCHAR (200)      NOT NULL,
    [UnitPrice]   DECIMAL (18, 2)     NOT NULL,
    [Quantity]    INT                 NOT NULL,
    CONSTRAINT [PK_OrderItems] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_OrderItems_Orders] FOREIGN KEY ([OrderId]) REFERENCES [ordering].[Orders] ([Id]),
    CONSTRAINT [CK_OrderItems_Quantity] CHECK ([Quantity] > (0)),
    CONSTRAINT [CK_OrderItems_UnitPrice] CHECK ([UnitPrice] > (0))
);
GO

CREATE NONCLUSTERED INDEX [IX_OrderItems_OrderId]
    ON [ordering].[OrderItems] ([OrderId] ASC);
