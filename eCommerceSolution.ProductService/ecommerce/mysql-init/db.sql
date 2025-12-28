CREATE DATABASE IF NOT EXISTS eCommerceProductServiceDB;

USE eCommerceProductServiceDB;

-- Create product table
CREATE TABLE Products (
    ProductID char(36) NOT NULL,
    ProductName varchar(50) NOT NULL,
    Category varchar(50) NOT NULL,
    UnitPrice decimal(10,2) DEFAULT NULL,
    QuantityInStock int DEFAULT NULL,
    PRIMARY KEY(ProductID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

INSERT INTO Products(ProductID, ProductName, Category, UnitPrice, QuantityInStock)
VALUES 
('1a9df78b-3f46-4c3d-9f2a-1b9f69292a77', 'Apple Iphone', 'Electronics', 2000.99, 100),
('2c8e8e7c-97a3-4b11-9a1b-4dbe681cfe17', 'Samsung phone', 'Electronics', 1299.88, 200),
('3f3e8b3a-4a50-4cd0-8d8e-1e178ae2cfc1', 'Chair', 'Furniture', 299.77, 399),
('4c9b6f71-6c5d-485f-8db2-58011a236b63', 'Storage', 'Furniture', 399.88, 500),
('5d7e36bf-65c3-4a71-bf97-740d561d8b65', 'TV', 'Electronics', 66.88, 200),
('6a14f510-72c1-42c8-9a5a-8ef8f3f45a0d', 'Shoes', 'Furniture', 12.88, 900),
('7b39ef14-932b-4c84-9187-55b748d2b28f', 'Laptop backpage', 'Accessories', 60.6, 700);