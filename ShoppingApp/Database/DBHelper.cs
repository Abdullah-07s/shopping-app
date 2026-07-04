using System;
using System.Data;
using System.Data.SqlClient;

namespace ShoppingApp.Database
{
    public static class DBHelper
    {
        private static string connectionString =
            @"Server=.\SQLEXPRESS;Database=ShoppingAppDB;Integrated Security=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public static void SetConnectionString(string server, string database,
                                               string user = "", string pass = "")
        {
            if (string.IsNullOrEmpty(user))
                connectionString = $@"Server={server};Database={database};Integrated Security=True;";
            else
                connectionString = $@"Server={server};Database={database};
                                      User Id={user};Password={pass};";
        }

        public static bool TestConnection()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch { return false; }
        }

        public static void InitializeDatabase()
        {
            string[] scripts = {
                // Users table
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
                  CREATE TABLE Users (
                      UserID       INT PRIMARY KEY IDENTITY(1,1),
                      Username     NVARCHAR(50)  NOT NULL UNIQUE,
                      Password     NVARCHAR(255) NOT NULL,
                      FullName     NVARCHAR(100) NOT NULL,
                      Email        NVARCHAR(100),
                      CreatedAt    DATETIME DEFAULT GETDATE(),
                      IsAdmin      BIT DEFAULT 0
                  );",

                // Categories table
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Categories' AND xtype='U')
                  CREATE TABLE Categories (
                      CategoryID   INT PRIMARY KEY IDENTITY(1,1),
                      Name         NVARCHAR(50) NOT NULL,
                      Icon         NVARCHAR(10)
                  );",

                // Products table
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Products' AND xtype='U')
                  CREATE TABLE Products (
                      ProductID    INT PRIMARY KEY IDENTITY(1,1),
                      Name         NVARCHAR(100) NOT NULL,
                      Description  NVARCHAR(500),
                      Price        DECIMAL(10,2) NOT NULL,
                      Stock        INT DEFAULT 0,
                      CategoryID   INT FOREIGN KEY REFERENCES Categories(CategoryID),
                      ImagePath    NVARCHAR(255),
                      CreatedAt    DATETIME DEFAULT GETDATE()
                  );",

                // Orders table
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Orders' AND xtype='U')
                  CREATE TABLE Orders (
                      OrderID      INT PRIMARY KEY IDENTITY(1,1),
                      UserID       INT FOREIGN KEY REFERENCES Users(UserID),
                      TotalAmount  DECIMAL(10,2) NOT NULL,
                      Status       NVARCHAR(20) DEFAULT 'Pending',
                      Address      NVARCHAR(255),
                      OrderDate    DATETIME DEFAULT GETDATE()
                  );",

                // OrderItems table
                @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='OrderItems' AND xtype='U')
                  CREATE TABLE OrderItems (
                      ItemID       INT PRIMARY KEY IDENTITY(1,1),
                      OrderID      INT FOREIGN KEY REFERENCES Orders(OrderID),
                      ProductID    INT FOREIGN KEY REFERENCES Products(ProductID),
                      Quantity     INT NOT NULL,
                      UnitPrice    DECIMAL(10,2) NOT NULL
                  );",

                // Seed default admin
                @"IF NOT EXISTS (SELECT * FROM Users WHERE Username='admin')
                  INSERT INTO Users (Username, Password, FullName, Email, IsAdmin)
                  VALUES ('admin', 'admin123', 'Administrator', 'admin@shop.com', 1);",

                // Seed categories
                @"IF NOT EXISTS (SELECT * FROM Categories)
                  BEGIN
                      INSERT INTO Categories (Name, Icon) VALUES
                      ('Electronics', '💻'),
                      ('Clothing',    '👕'),
                      ('Books',       '📚'),
                      ('Food',        '🍕'),
                      ('Sports',      '⚽');
                  END",

                // Seed sample products
                @"IF NOT EXISTS (SELECT * FROM Products)
                  BEGIN
                      INSERT INTO Products (Name, Description, Price, Stock, CategoryID) VALUES
                      ('Laptop Pro 15',    'High-performance laptop, 16GB RAM, 512GB SSD', 149999, 10, 1),
                      ('Wireless Earbuds', 'Active noise cancellation, 24hr battery',       7999, 50, 1),
                      ('USB-C Hub 7-in-1', 'HDMI, USB 3.0, SD card, PD charging',           2499, 30, 1),
                      ('Cotton T-Shirt',   'Premium cotton, available in all sizes',          899, 100, 2),
                      ('Denim Jacket',     'Classic slim-fit denim, multiple colors',        3499, 40, 2),
                      ('Running Shoes',    'Lightweight, breathable mesh upper',             5999, 25, 2),
                      ('Clean Code',       'Robert C. Martin – must read for developers',    1299, 20, 3),
                      ('The Pragmatic Programmer', 'Hunt & Thomas – timeless software wisdom', 1499, 15, 3),
                      ('Football',         'FIFA approved match ball',                       2999, 60, 5),
                      ('Yoga Mat',         'Non-slip, eco-friendly, 6mm thick',              1799, 35, 5);
                  END"
            };

            using (var conn = GetConnection())
            {
                conn.Open();
                foreach (var script in scripts)
                {
                    using (var cmd = new SqlCommand(script, conn))
                        cmd.ExecuteNonQuery();
                }
            }
        }

        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            var dt = new DataTable();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);
                    using (var adapter = new SqlDataAdapter(cmd))
                        adapter.Fill(dt);
                }
            }
            return dt;
        }

        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public static object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);
                    return cmd.ExecuteScalar();
                }
            }
        }
    }
}
