# Web Application with PostgreSQL Integration

This ASP.NET Core web application demonstrates integration with PostgreSQL database to display product data.

## Features

- Pulls data from PostgreSQL database using raw SQL queries
- Displays products in a responsive table format
- No Entity Framework migrations used

## Prerequisites

- .NET 10.0 SDK
- PostgreSQL database
- Docker (optional, for easy PostgreSQL setup)

## Setup Instructions

### Option 1: Using Docker for PostgreSQL (Recommended)

1. Start PostgreSQL container:
```bash
docker-compose -f docker-compose.db.yml up -d
```

2. The database will be automatically initialized with sample data from `init-database.sql`

### Option 2: Manual PostgreSQL Setup

1. Install and start PostgreSQL on your system
2. Create database:
```sql
CREATE DATABASE webapp_db;
```

3. Run the initialization script:
```bash
psql -U postgres -d webapp_db -f init-database.sql
```

## Running the Application

1. Restore NuGet packages:
```bash
dotnet restore
```

2. Build the application:
```bash
dotnet build
```

3. Run the application:
```bash
cd WebAppNoAuth

dotnet watch run
```
Option watch gives hot reload

4. Open your browser and navigate to `https://localhost:5033`
5. You can also run it in docker with 
```docker-compose -f compose.yaml up 
```
It will listen on port 8080

## Database Schema

The application uses a simple `products` table with the following structure:

```sql
CREATE TABLE products (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    price DECIMAL(10, 2) NOT NULL,
    category VARCHAR(50),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

## Project Structure

- `Models/Product.cs` - Data model representing products
- `Services/ProductService.cs` - Database service with raw SQL operations
- `Controllers/HomeController.cs` - Main controller fetching and displaying data
- `Views/Home/Index.cshtml` - View template displaying products in a table
- `init-database.sql` - SQL script to create table and sample data

## Connection String

The default connection string is configured in `appsettings.json`:
```
Host=localhost;Port=5432;Database=webapp_db;Username=postgres;Password=postgres
```

Update this string according to your PostgreSQL configuration.

## Added UserController to access user list maintained in memory
- Get all users:
```
curl -s http://localhost:5033/api/user | jq
```
- Get user by username:
```
curl -s http://localhost:5033/api/user/admin | jq
```
- Add a user:
```
curl -s -X POST http://localhost:5033/api/user -H "Content-Type: application/json" -d '{"username":"test_user","email":"test@example.com","location":"Test City","department":"Testing","role":"Tester"}' | jq
```

- Get token for a user and request a resource based on that token (urls optional):
```
./jwt.sh username resource_url token_url
```

## AdminController uses role authorization based on AuthUser.Role obtained through AuthUser.Username in JWT token

## To move your sensitive config values to secrets:
```
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=DBName;Username=username;Password=yourpassword"
dotnet user-secrets set "Jwt:Key" "ThisYourSecretKeyForJWTAuthentication"
```

## Known Issues

- Bootstrap and jQuery are not loaded as dependencies
- connection pool - replace DbContext with DbContextPool
- authentification - jwt info needs to be moved to Secret Manager !
- db login info needs to be moved to Secret Manager !
