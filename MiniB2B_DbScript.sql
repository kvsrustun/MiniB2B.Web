IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'MiniB2BDb')
BEGIN
    CREATE DATABASE MiniB2BDb;
END
GO

USE MiniB2BDb;
GO

IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL UNIQUE,
        Email NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL,
        FirstName NVARCHAR(50) NOT NULL,
        LastName NVARCHAR(50) NOT NULL,
        PhoneNumber NVARCHAR(50) NULL,
        Role NVARCHAR(20) NOT NULL DEFAULT 'Dealer',
        CreatedAt DATETIME2 DEFAULT GETDATE(),
        IsActive BIT DEFAULT 1
    );
END
GO

IF OBJECT_ID('dbo.Categories', 'U') IS NULL
BEGIN
    CREATE TABLE Categories (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        IsActive BIT DEFAULT 1
    );
END
GO

IF OBJECT_ID('dbo.Products', 'U') IS NULL
BEGIN
    CREATE TABLE Products (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ProductCode NVARCHAR(50) NOT NULL UNIQUE,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        Brand NVARCHAR(100) NOT NULL,
        ManufacturerCode NVARCHAR(100) NULL,
        SpecialCode1 NVARCHAR(50) NULL,
        SpecialCode2 NVARCHAR(50) NULL,
        ImageUrl NVARCHAR(500) NULL,
        StockQuantity INT NOT NULL DEFAULT 0,
        CriticalStockThreshold INT NOT NULL DEFAULT 10,
        Price DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        CategoryId INT NULL,
        IsActive BIT DEFAULT 1,
        CreatedAt DATETIME2 DEFAULT GETDATE(),
        CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
    );

    CREATE NONCLUSTERED INDEX IX_Products_ProductCode ON Products(ProductCode);
    CREATE NONCLUSTERED INDEX IX_Products_Brand ON Products(Brand);
    CREATE NONCLUSTERED INDEX IX_Products_Name ON Products(Name);
END
GO

IF OBJECT_ID('dbo.GridColumnConfigs', 'U') IS NULL
BEGIN
    CREATE TABLE GridColumnConfigs (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FieldName NVARCHAR(50) NOT NULL,
        HeaderTitle NVARCHAR(100) NOT NULL,
        DisplayOrder INT NOT NULL,
        RenderType NVARCHAR(50) NOT NULL,
        Width NVARCHAR(20) DEFAULT 'auto',
        IsVisible BIT DEFAULT 1,
        Alignment NVARCHAR(20) DEFAULT 'left'
    );
END
GO

IF OBJECT_ID('dbo.Carts', 'U') IS NULL
BEGIN
    CREATE TABLE Carts (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL UNIQUE,
        UpdatedAt DATETIME2 DEFAULT GETDATE(),
        CONSTRAINT FK_Carts_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID('dbo.CartItems', 'U') IS NULL
BEGIN
    CREATE TABLE CartItems (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CartId INT NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL CHECK (Quantity > 0),
        CreatedAt DATETIME2 DEFAULT GETDATE(),
        CONSTRAINT FK_CartItems_Carts FOREIGN KEY (CartId) REFERENCES Carts(Id) ON DELETE CASCADE,
        CONSTRAINT FK_CartItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
    );
END
GO

IF OBJECT_ID('dbo.Orders', 'U') IS NULL
BEGIN
    CREATE TABLE Orders (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrderNumber NVARCHAR(50) NOT NULL UNIQUE,
        UserId INT NOT NULL,
        OrderDate DATETIME2 DEFAULT GETDATE(),
        TotalAmount DECIMAL(18,2) NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'Onay Bekliyor',
        CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
    );
END
GO

IF OBJECT_ID('dbo.OrderItems', 'U') IS NULL
BEGIN
    CREATE TABLE OrderItems (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrderId INT NOT NULL,
        ProductId INT NOT NULL,
        ProductCodeSnapshot NVARCHAR(50) NOT NULL,
        ProductNameSnapshot NVARCHAR(200) NOT NULL,
        UnitPriceSnapshot DECIMAL(18,2) NOT NULL,
        Quantity INT NOT NULL,
        TotalPrice DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
        CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
    );
END
GO

IF OBJECT_ID('dbo.Banners', 'U') IS NULL
BEGIN
    CREATE TABLE Banners (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(100) NOT NULL,
        Description NVARCHAR(250) NULL,
        BadgeText NVARCHAR(50) NULL,
        BadgeColor NVARCHAR(50) NULL,
        BackgroundGradient NVARCHAR(200) NULL,
        ImageUrl NVARCHAR(500) NULL,
        DisplayOrder INT NOT NULL DEFAULT 1,
        IsActive BIT NOT NULL DEFAULT 1
    );
END
GO

INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, PhoneNumber, Role)
VALUES 
('admin', 'admin@b2b.com', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 'Sistem', 'Yöneticisi', '05551112233', 'Admin'),
('bayi1', 'bayi1@b2b.com', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 'Ahmet', 'Yılmaz', '05554445566', 'Dealer');

INSERT INTO Categories (Name, IsActive) VALUES 
('Endüstriyel Parçalar', 1), 
('Hırdavat & Civata', 1), 
('Elektrik & Otomasyon', 1);

INSERT INTO GridColumnConfigs (FieldName, HeaderTitle, DisplayOrder, RenderType, Width, Alignment, IsVisible)
VALUES 
('ImageUrl', 'Görsel', 1, 'Image', '70px', 'center', 1),
('ProductCode', 'Ürün Kodu', 2, 'Text', '120px', 'left', 1),
('Name', 'Ürün Adı', 3, 'Text', 'auto', 'left', 1),
('Brand', 'Marka', 4, 'Text', '110px', 'left', 1),
('StockStatus', 'Stok Durumu', 5, 'StockBadge', '110px', 'center', 1),
('Price', 'Fiyat', 6, 'Price', '120px', 'right', 1),
('Action', 'Sipariş Girişi', 7, 'ActionCart', '150px', 'center', 1);

INSERT INTO Products (ProductCode, Name, Description, Brand, ManufacturerCode, SpecialCode1, SpecialCode2, ImageUrl, StockQuantity, CriticalStockThreshold, Price, CategoryId, IsActive)
VALUES 
('PRD-1001', 'Rulman 6204-2RS C3 Yüksek Devir', 'Yüksek devirli motorlar için bilyalı rulman.', 'SKF', 'SKF-6204-2RSH/C3', 'MEK-RLM', 'RAF-A-04', 'https://images.unsplash.com/photo-1581092160607-ee22621dd758?w=200', 45, 10, 185.50, 1, 1),
('PRD-1002', 'Pnömatik Silindir 32x100 Çift Etkili', 'ISO 15552 standartlarında manyetik silindir.', 'Festo', 'FESTO-DSBC-32-100-PPVA', 'PNO-AKT', 'RAF-B-12', 'https://images.unsplash.com/photo-1581092335397-9583fe92d232?w=200', 4, 10, 1450.00, 1, 1),
('PRD-1003', 'Endüktif Sensör M12 PNP NO', '4mm algılama mesafeli IP67 yaklaşım sensörü.', 'Omron', 'OMR-E2B-M12KS04-WP-B1', 'ELEK-SNS', 'RAF-C-01', 'https://images.unsplash.com/photo-1518770660439-4636190af475?w=200', 0, 5, 620.00, 3, 1),
('PRD-1004', 'M8x40 İmbus Civata 8.8 Kalite', 'DIN 912 alyan başlı çelik bağlantı civatası.', 'Norm Civata', 'NRM-DIN912-88-M840', 'BAG-HRD', 'KUTU-18', 'https://images.unsplash.com/photo-1530124566582-a618bc2615dc?w=200', 120, 20, 310.00, 2, 1),
('PRD-1005', 'Motor Koruma Şalteri 4-6.3A', 'Termik manyetik açma korumalı kompakt şalter.', 'Schneider', 'SCH-GV2ME10-63A', 'PANO-SLT', 'RAF-D-08', 'https://images.unsplash.com/photo-1581092580497-e0d23cbdf1dc?w=200', 12, 5, 890.00, 3, 1);

INSERT INTO Banners (Title, Description, BadgeText, BadgeColor, BackgroundGradient, ImageUrl, DisplayOrder, IsActive)
VALUES 
('Endüstriyel Motor ve Şalterlerde %15 Toptan İndirim', 'Schneider ve Siemens şalt ürünlerinde bayilere özel avantaj.', 'ÖZEL BAYİ KAMPANYASI', 'warning', 'linear-gradient(135deg, #1e3c72 0%, #2a5298 100%)', 'https://images.unsplash.com/photo-1581092160607-ee22621dd758?w=500', 1, 1),
('SKF & FAG Yüksek Hızlı Rulman Serileri Depolarımızda!', 'Yedek parça ve rulman depolarımız güncellendi.', 'YENİ STOK GİRİŞİ', 'success', 'linear-gradient(135deg, #0f2027 0%, #203a43 50%, #2c5364 100%)', 'https://images.unsplash.com/photo-1581092335397-9583fe92d232?w=500', 2, 1),
('Saat 16:00''a Kadar Verilen Siparişler Aynı Gün Kargoda', 'B2B portalı üzerinden siparişinizi tamamlayın, anında sevkiyat başlasın.', 'ÖNEMLİ DUYURU', 'danger', 'linear-gradient(135deg, #373b44 0%, #4286f4 100%)', 'https://images.unsplash.com/photo-1616401784845-180882ba9ba8?w=500', 3, 1);
GO