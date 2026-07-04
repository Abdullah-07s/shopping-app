# 🛍️ ShopEase — Shopping Application

A full-featured desktop shopping application built with **C# Windows Forms** and **SQL Server**. Features a modern dark-themed UI, product catalogue with search and category filters, shopping cart, checkout, order history, and user authentication.

---

## ✨ Features

### 🔐 Authentication
- User registration with validation (unique username, password confirmation, min length)
- Secure login with session management
- Admin account pre-seeded (`admin` / `admin123`)

### 🏪 Product Catalogue
- Browse 10+ pre-loaded products across 5 categories (Electronics, Clothing, Books, Food, Sports)
- **Real-time search** — filter products by name or description as you type
- **Category sidebar** — filter by category with one click
- Product cards showing name, description, price (in Rs.), and stock level

### 🛒 Shopping Cart
- Add items to cart with visual feedback (button turns green briefly)
- Adjust quantities with `+` / `−` buttons
- Remove individual items or clear entire cart
- Live subtotal per item and running cart total

### 💳 Checkout
- Order summary with all cart items
- Delivery address input
- One-click order confirmation
- Stock automatically reduced after order
- Order ID assigned and displayed on success

### 📦 Order History
- View all past orders in a sortable grid
- Order date, total, status, and delivery address shown

### 🗄️ Database
- Auto-creates all tables and seeds sample data on first run
- No manual SQL setup required

---

## 🏗️ Architecture

```
ShoppingApp/
├── Program.cs                      # Entry point + DB init
├── Database/
│   └── DBHelper.cs                 # SQL Server connection, queries, schema init
├── Models/
│   └── Models.cs                   # User, Product, CartItem, Order, Session
└── Forms/
    ├── LoginForm.cs                 # Login screen
    ├── RegisterForm.cs              # Registration screen
    ├── MainForm.cs                  # Product catalogue + search + categories
    ├── CartForm.cs                  # Cart management
    └── CheckoutOrdersForms.cs       # Checkout + Order history
```

### Database Schema
```
Users          → UserID, Username, Password, FullName, Email, IsAdmin
Categories     → CategoryID, Name, Icon
Products       → ProductID, Name, Description, Price, Stock, CategoryID
Orders         → OrderID, UserID, TotalAmount, Status, Address, OrderDate
OrderItems     → ItemID, OrderID, ProductID, Quantity, UnitPrice
```

---

## ⚙️ Setup & Run

### Prerequisites
- **Visual Studio 2022** (or VS 2019 with .NET 6 support)
- **SQL Server Express** — [Download free](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- **.NET 6.0 SDK** — [Download](https://dotnet.microsoft.com/download/dotnet/6.0)

### Steps

1. **Clone the repo:**
   ```bash
   git clone https://github.com/Abdullah-07s/shopping-app.git
   cd shopping-app
   ```

2. **Start SQL Server Express** — make sure the service is running (check Windows Services or SQL Server Configuration Manager).

3. **Open in Visual Studio:**
   - Open `ShoppingApp/ShoppingApp.csproj`
   - Visual Studio will restore the `Microsoft.Data.SqlClient` NuGet package automatically

4. **Run the app** (`F5` or `Ctrl+F5`)
   - On first run, the app auto-creates the `ShoppingAppDB` database, all tables, and seeds sample data.
   - If it can't connect, check that your SQL Server instance name is `.\SQLEXPRESS`. To change it, edit `connectionString` in `Database/DBHelper.cs`.

5. **Login:**
   - Default admin: `admin` / `admin123`
   - Or register a new account

---

## 🎨 UI Design

Dark-themed modern interface built entirely in WinForms:
- **Colour palette:** Deep navy background (`#0D0D17`), card panels (`#1C1C2D`), blue accent (`#4895EF`), green for prices and success
- **Typography:** Segoe UI throughout, weight used for hierarchy
- **Sidebar** category navigation with active state highlighting
- **Product cards** with hover feedback and animated "Added!" button state

---

## 🛠️ Tech Stack

![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=csharp&logoColor=white)
![WinForms](https://img.shields.io/badge/Windows_Forms-GUI-0078D4?style=flat-square&logo=windows&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=flat-square&logo=microsoftsqlserver&logoColor=white)
![.NET](https://img.shields.io/badge/.NET_6-512BD4?style=flat-square&logo=dotnet&logoColor=white)

---
