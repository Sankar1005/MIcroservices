/*
    Post-deployment script: runs after every publish of Microservices.Database.
    Only sample data belongs here, and every block must be safe to run again.
*/

-- ProductService sample products (only inserted when the table is empty).
IF NOT EXISTS (SELECT 1 FROM [product].[Products])
BEGIN
    INSERT INTO [product].[Products] ([Name], [Description], [Price], [StockQuantity])
    VALUES (N'Laptop',              N'14-inch business laptop, 16 GB RAM, 512 GB SSD', 65000.00,  25),
           (N'Wireless Mouse',      N'Ergonomic 2.4 GHz wireless mouse',                 899.00, 200),
           (N'Mechanical Keyboard', N'Tenkeyless keyboard with blue switches',          3499.00,  80),
           (N'27-inch Monitor',     N'QHD IPS monitor, 75 Hz',                         18999.00,  30),
           (N'USB-C Hub',           N'7-in-1 hub with HDMI and SD card reader',         1999.00, 120);
END
