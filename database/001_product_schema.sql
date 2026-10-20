-- ProductService: schema, table and sample data.
-- Safe to run more than once: every step checks whether it has already been done.
--
-- Run with:
--   sqlcmd -S "(localdb)\MSSQLLocalDB" -E -d Microservices -i database\001_product_schema.sql
USE Microservices;
GO

-- 1. Schema
IF SCHEMA_ID(N'product') IS NULL
    EXEC (N'CREATE SCHEMA [product]');
GO

-- 2. Table
IF OBJECT_ID(N'product.Products', N'U') IS NULL
BEGIN
    CREATE TABLE product.Products
    (
        Id            INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Products PRIMARY KEY,
        Name          NVARCHAR(200)      NOT NULL,
        Description   NVARCHAR(1000)     NULL,
        Price         DECIMAL(18, 2)     NOT NULL CONSTRAINT CK_Products_Price CHECK (Price > 0),
        StockQuantity INT                NOT NULL CONSTRAINT DF_Products_StockQuantity DEFAULT (0)
                                                  CONSTRAINT CK_Products_StockQuantity CHECK (StockQuantity >= 0),
        IsActive      BIT                NOT NULL CONSTRAINT DF_Products_IsActive DEFAULT (1),
        CreatedAt     DATETIME2          NOT NULL CONSTRAINT DF_Products_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt     DATETIME2          NULL
    );
END
GO

-- 3. Sample data (only when the table is empty)
IF NOT EXISTS (SELECT 1 FROM product.Products)
BEGIN
    INSERT INTO product.Products (Name, Description, Price, StockQuantity)
    VALUES (N'Laptop',              N'14-inch business laptop, 16 GB RAM, 512 GB SSD', 65000.00,  25),
           (N'Wireless Mouse',      N'Ergonomic 2.4 GHz wireless mouse',                 899.00, 200),
           (N'Mechanical Keyboard', N'Tenkeyless keyboard with blue switches',          3499.00,  80),
           (N'27-inch Monitor',     N'QHD IPS monitor, 75 Hz',                         18999.00,  30),
           (N'USB-C Hub',           N'7-in-1 hub with HDMI and SD card reader',         1999.00, 120);
END
GO
