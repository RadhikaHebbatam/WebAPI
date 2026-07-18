-- Schema for ProductCatalogueDB — run once against a fresh Azure SQL Database
-- (or any SQL Server instance) before pointing the API at it.

CREATE TABLE Categories (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    Name          NVARCHAR(100)  NOT NULL,
    Description   NVARCHAR(500)  NOT NULL DEFAULT '',
    IsActive      BIT            NOT NULL DEFAULT 1,
    CreatedAt     DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE Products (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    Name          NVARCHAR(200)  NOT NULL,
    Description   NVARCHAR(1000) NOT NULL DEFAULT '',
    Price         DECIMAL(10,2)  NOT NULL,
    StockQuantity INT            NOT NULL DEFAULT 0,
    IsActive      BIT            NOT NULL DEFAULT 1,
    CategoryId    INT            NOT NULL REFERENCES Categories(Id),
    CreatedAt     DATETIME2      NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt     DATETIME2      NULL
);

CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);

-- Sample seed data (matches the local dev dataset)
INSERT INTO Categories (Name, Description, IsActive) VALUES
    ('Electronics', 'Electronic gadgets and accessories', 1),
    ('Apparel', 'Clothing and footwear', 1),
    ('Books', 'Programming and technical books', 1),
    ('Home & Garden', 'Home and outdoor goods', 1),
    ('Fitness', 'Exercise and fitness equipment', 1);

INSERT INTO Products (Name, Description, Price, StockQuantity, IsActive, CategoryId) VALUES
    ('Wireless Mouse', 'Ergonomic wireless mouse with USB receiver', 29.99, 150, 1, 1),
    ('Mechanical Keyboard', 'RGB mechanical keyboard with blue switches', 89.99, 75, 1, 1),
    ('USB-C Hub', '7-in-1 USB-C hub with HDMI and ethernet', 49.99, 200, 1, 1),
    ('Running Shoes', 'Lightweight running shoes for all terrains', 119.99, 60, 1, 2),
    ('Cotton T-Shirt', 'Premium cotton t-shirt available in 5 colours', 24.99, 300, 1, 2),
    ('Clean Code', 'A handbook of agile software craftsmanship', 39.99, 100, 1, 3),
    ('The Pragmatic Programmer', 'Your journey to mastery', 44.99, 85, 1, 3),
    ('Garden Hose', '50ft expandable garden hose with spray nozzle', 34.99, 120, 1, 4),
    ('Yoga Mat', 'Non-slip yoga mat with carrying strap', 27.99, 180, 1, 5),
    ('Dumbbell Set', 'Adjustable dumbbell set 5-25kg', 199.99, 40, 1, 5);
