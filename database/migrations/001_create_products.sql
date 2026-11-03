-- 001_create_products.sql : crée la table des produits (si elle n'existe pas déjà)
IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products (
        Id        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Products PRIMARY KEY,
        Name      NVARCHAR(100)     NOT NULL,
        Price     DECIMAL(10,3)     NOT NULL CONSTRAINT CK_Products_Price CHECK (Price >= 0),
        CreatedAt DATETIME2         NOT NULL CONSTRAINT DF_Products_CreatedAt DEFAULT SYSUTCDATETIME()
    );
END