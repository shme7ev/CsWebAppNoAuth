-- Create database (uncomment if needed - usually created separately)
-- CREATE DATABASE webapp_db;

-- Connect to the database
-- \c webapp_db;

-- Drop table if exists
DROP TABLE IF EXISTS products;

-- Create products table
CREATE TABLE products (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    price DECIMAL(10, 2) NOT NULL,
    category VARCHAR(50),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Insert sample data
INSERT INTO products (name, description, price, category) VALUES
('Laptop Computer', 'High-performance laptop with 16GB RAM and 512GB SSD', 1299.99, 'Electronics'),
('Wireless Mouse', 'Ergonomic wireless mouse with USB receiver', 29.99, 'Electronics'),
('Coffee Mug', 'Ceramic coffee mug with company logo', 12.50, 'Office Supplies'),
('Desk Lamp', 'Adjustable LED desk lamp with touch controls', 45.00, 'Office Supplies'),
('Notebook', 'Spiral-bound notebook with 100 pages', 8.99, 'Stationery'),
('Bluetooth Headphones', 'Noise-cancelling wireless headphones', 199.99, 'Electronics'),
('Water Bottle', 'Stainless steel water bottle 32oz', 24.99, 'Accessories'),
('Backpack', 'Water-resistant backpack with laptop compartment', 59.99, 'Accessories');

-- Verify data insertion
SELECT COUNT(*) as total_products FROM products;
SELECT * FROM products ORDER BY name;