# EasyGames — Project Overview

EasyGames is a small ASP.NET Core MVC web application that simulates an online
store for collectible games and related products.  
It was developed as a HIT339 Assessment 2 team project to practise .NET, EF Core,
web app architecture, and basic e-commerce functionality.

---

## Main Features

- **Public site**
  - Landing page
  - Store with search and category filtering
  - Product cards with consistent image ratios
  - Cart and checkout demo flow

- **Admin area (owner only)**
  - Product management (create, read, update, delete)
  - User management (create, read, update, delete)
  - Owner-only access using authorisation attributes

- **Cart & orders**
  - Session-backed cart service
  - Quantity updates and removesthe  item
  - Demo checkout that creates an `Order` with code format `EG-YYYYMMDD-####`

---

## Tech Stack

- **Framework:** ASP.NET Core MVC (.NET)
- **Data:** Entity Framework Core with SQLite
- **Auth:** Cookie authentication with users stored in EF Core
- **UI:** Razor views, Bootstrap, and custom CSS
- **Other:**
  - Idempotent data seeding for products, users, and sample orders
  - Owner bootstrap via environment variables
  - Basic routing with public and Admin areas
  - Optional reduced-motion support for subtle visual effects

---

## Data Model (simplified)

- **Product**
  - `Id`, `Title`, `Category` (enum), `Price`, `StockQty`, `ImageUrl`, etc.
- **User**
  - `Id`, `Email`, `DisplayName`, `PasswordHash`, `Role`
- **Order / OrderItem**
  - Records checkout information and line items

---

## Running the Application

1. Clone the repository.
2. Open the solution in Visual Studio 2022 (or use the `dotnet` CLI).
3. Restore NuGet packages and build the solution.
4. Run the web project:
   ```bash
   dotnet run
