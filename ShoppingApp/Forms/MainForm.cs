using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using ShoppingApp.Database;
using ShoppingApp.Models;

namespace ShoppingApp.Forms
{
    public class MainForm : Form
    {
        // ── Colours ───────────────────────────────────────────────────────────
        static readonly Color BG        = Color.FromArgb(13, 13, 23);
        static readonly Color SIDEBAR   = Color.FromArgb(20, 20, 35);
        static readonly Color CARD      = Color.FromArgb(28, 28, 45);
        static readonly Color ACCENT    = Color.FromArgb(99, 179, 237);
        static readonly Color ACCENT2   = Color.FromArgb(72, 149, 239);
        static readonly Color GREEN     = Color.FromArgb(104, 211, 145);
        static readonly Color TEXT      = Color.FromArgb(240, 240, 255);
        static readonly Color MUTED     = Color.FromArgb(140, 140, 170);
        static readonly Color INPUT_BG  = Color.FromArgb(35, 35, 55);

        // ── Controls ─────────────────────────────────────────────────────────
        private Panel pnlSidebar, pnlMain, pnlTopbar;
        private TextBox txtSearch;
        private FlowLayoutPanel flpProducts;
        private Label lblCartCount, lblWelcome, lblCategory;
        private Button btnCart, btnOrders, btnLogout;
        private List<Button> categoryButtons = new List<Button>();

        private int selectedCategoryID = 0; // 0 = All

        public MainForm()
        {
            InitializeComponent();
            LoadCategories();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.Text            = "ShopEase";
            this.Size            = new Size(1280, 800);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.BackColor       = BG;
            this.MinimumSize     = new Size(1000, 650);

            // ── Top bar ───────────────────────────────────────────────────────
            pnlTopbar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 60,
                BackColor = SIDEBAR
            };

            var lblBrand = new Label
            {
                Text      = "🛍️ ShopEase",
                Font      = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = ACCENT,
                AutoSize  = true,
                Location  = new Point(20, 16)
            };

            txtSearch = new TextBox
            {
                Size        = new Size(380, 34),
                Location    = new Point(200, 14),
                BackColor   = INPUT_BG,
                ForeColor   = TEXT,
                Font        = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += (s, e) => LoadProducts();

            var btnSearchIcon = new Label
            {
                Text      = "🔍",
                Font      = new Font("Segoe UI", 12),
                AutoSize  = true,
                Location  = new Point(590, 18),
                ForeColor = MUTED,
                BackColor = Color.Transparent
            };

            lblWelcome = new Label
            {
                Text      = $"Hi, {Session.CurrentUser?.FullName ?? "Guest"} 👋",
                Font      = new Font("Segoe UI", 10),
                ForeColor = MUTED,
                AutoSize  = true,
                Location  = new Point(640, 20)
            };

            btnCart = new Button
            {
                Text      = "🛒 Cart (0)",
                Size      = new Size(130, 36),
                Location  = new Point(950, 12),
                BackColor = ACCENT2,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnCart.FlatAppearance.BorderSize = 0;
            btnCart.Click += (s, e) => { new CartForm().ShowDialog(); UpdateCartButton(); LoadProducts(); };

            btnOrders = new Button
            {
                Text      = "📦 My Orders",
                Size      = new Size(120, 36),
                Location  = new Point(1090, 12),
                BackColor = Color.FromArgb(50, 50, 80),
                ForeColor = TEXT,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10),
                Cursor    = Cursors.Hand
            };
            btnOrders.FlatAppearance.BorderSize = 0;
            btnOrders.Click += (s, e) => new OrdersForm().ShowDialog();

            btnLogout = new Button
            {
                Text      = "Logout",
                Size      = new Size(80, 36),
                Location  = new Point(1180, 12),
                BackColor = Color.FromArgb(80, 30, 30),
                ForeColor = Color.FromArgb(252, 129, 129),
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9),
                Cursor    = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) => {
                Session.Logout();
                new LoginForm().Show();
                this.Close();
            };

            pnlTopbar.Controls.AddRange(new Control[] {
                lblBrand, txtSearch, btnSearchIcon,
                lblWelcome, btnCart, btnOrders, btnLogout
            });
            this.Controls.Add(pnlTopbar);

            // ── Sidebar (categories) ──────────────────────────────────────────
            pnlSidebar = new Panel
            {
                Dock      = DockStyle.Left,
                Width     = 200,
                BackColor = SIDEBAR,
                Padding   = new Padding(0, 10, 0, 0)
            };

            var lblCatHeader = new Label
            {
                Text      = "CATEGORIES",
                Font      = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = MUTED,
                AutoSize  = true,
                Location  = new Point(20, 15)
            };
            pnlSidebar.Controls.Add(lblCatHeader);

            this.Controls.Add(pnlSidebar);

            // ── Main products area ────────────────────────────────────────────
            pnlMain = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = BG,
                Padding   = new Padding(20, 15, 20, 20),
                AutoScroll = true
            };

            lblCategory = new Label
            {
                Text      = "All Products",
                Font      = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = TEXT,
                AutoSize  = true,
                Location  = new Point(20, 10)
            };
            pnlMain.Controls.Add(lblCategory);

            flpProducts = new FlowLayoutPanel
            {
                Location    = new Point(0, 50),
                AutoSize    = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                Padding      = new Padding(15, 0, 15, 15),
                BackColor    = BG
            };
            pnlMain.Controls.Add(flpProducts);

            this.Controls.Add(pnlMain);
            this.Controls.SetChildIndex(pnlTopbar, 0);
        }

        // ── Data loading ──────────────────────────────────────────────────────
        private void LoadCategories()
        {
            var dt = DBHelper.ExecuteQuery("SELECT * FROM Categories ORDER BY CategoryID");

            int y = 45;
            // "All" button
            var btnAll = MakeCategoryButton("🏠  All Products", new Point(10, y), 0);
            btnAll.BackColor = ACCENT2;
            pnlSidebar.Controls.Add(btnAll);
            categoryButtons.Add(btnAll);
            y += 44;

            foreach (System.Data.DataRow row in dt.Rows)
            {
                string name = $"{row["Icon"]}  {row["Name"]}";
                int id = (int)row["CategoryID"];
                var btn = MakeCategoryButton(name, new Point(10, y), id);
                pnlSidebar.Controls.Add(btn);
                categoryButtons.Add(btn);
                y += 44;
            }
        }

        private Button MakeCategoryButton(string text, Point loc, int categoryID)
        {
            var btn = new Button
            {
                Text      = text,
                Size      = new Size(178, 38),
                Location  = loc,
                BackColor = Color.FromArgb(35, 35, 58),
                ForeColor = TEXT,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10),
                Cursor    = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(10, 0, 0, 0),
                Tag       = categoryID
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) =>
            {
                selectedCategoryID = (int)btn.Tag;
                foreach (var b in categoryButtons)
                    b.BackColor = Color.FromArgb(35, 35, 58);
                btn.BackColor = ACCENT2;
                lblCategory.Text = selectedCategoryID == 0 ? "All Products" : btn.Text.Trim();
                LoadProducts();
            };
            return btn;
        }

        private void LoadProducts()
        {
            flpProducts.Controls.Clear();
            string search = txtSearch.Text.Trim();

            string query = @"SELECT p.*, c.Name as CategoryName, c.Icon as CategoryIcon
                             FROM Products p
                             LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                             WHERE p.Stock > 0";

            var paramList = new System.Collections.Generic.List<SqlParameter>();

            if (selectedCategoryID > 0)
            {
                query += " AND p.CategoryID = @cid";
                paramList.Add(new SqlParameter("@cid", selectedCategoryID));
            }
            if (!string.IsNullOrEmpty(search))
            {
                query += " AND (p.Name LIKE @s OR p.Description LIKE @s)";
                paramList.Add(new SqlParameter("@s", $"%{search}%"));
            }
            query += " ORDER BY p.ProductID";

            var dt = DBHelper.ExecuteQuery(query, paramList.ToArray());

            if (dt.Rows.Count == 0)
            {
                flpProducts.Controls.Add(new Label {
                    Text      = "No products found.",
                    Font      = new Font("Segoe UI", 12),
                    ForeColor = MUTED,
                    AutoSize  = true,
                    Margin    = new Padding(20)
                });
                return;
            }

            foreach (System.Data.DataRow row in dt.Rows)
            {
                var product = new Product
                {
                    ProductID    = (int)row["ProductID"],
                    Name         = row["Name"].ToString(),
                    Description  = row["Description"].ToString(),
                    Price        = (decimal)row["Price"],
                    Stock        = (int)row["Stock"],
                    CategoryName = row["CategoryName"].ToString(),
                    CategoryIcon = row["CategoryIcon"].ToString()
                };
                flpProducts.Controls.Add(CreateProductCard(product));
            }
        }

        private Panel CreateProductCard(Product product)
        {
            var card = new Panel
            {
                Size      = new Size(240, 300),
                BackColor = CARD,
                Margin    = new Padding(8),
                Cursor    = Cursors.Hand
            };

            // Category badge
            var lblCat = new Label
            {
                Text      = $"{product.CategoryIcon} {product.CategoryName}",
                Font      = new Font("Segoe UI", 8),
                ForeColor = ACCENT,
                AutoSize  = true,
                Location  = new Point(12, 12),
                BackColor = Color.FromArgb(20, 50, 80)
            };

            // Product name
            var lblName = new Label
            {
                Text      = product.Name,
                Font      = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = TEXT,
                Size      = new Size(216, 50),
                Location  = new Point(12, 42),
                BackColor = Color.Transparent
            };

            // Description
            var lblDesc = new Label
            {
                Text      = product.Description,
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = MUTED,
                Size      = new Size(216, 60),
                Location  = new Point(12, 98),
                BackColor = Color.Transparent
            };

            // Price
            var lblPrice = new Label
            {
                Text      = product.PriceFormatted,
                Font      = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = GREEN,
                AutoSize  = true,
                Location  = new Point(12, 168),
                BackColor = Color.Transparent
            };

            // Stock
            var lblStock = new Label
            {
                Text      = $"In stock: {product.Stock}",
                Font      = new Font("Segoe UI", 8),
                ForeColor = MUTED,
                AutoSize  = true,
                Location  = new Point(12, 205),
                BackColor = Color.Transparent
            };

            // Add to cart button
            var btnAdd = new Button
            {
                Text      = "+ Add to Cart",
                Size      = new Size(216, 40),
                Location  = new Point(12, 245),
                BackColor = ACCENT2,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, e) =>
            {
                Session.AddToCart(product, 1);
                UpdateCartButton();
                btnAdd.Text      = "✔ Added!";
                btnAdd.BackColor = GREEN;
                btnAdd.ForeColor = Color.FromArgb(20, 40, 20);
                var t = new System.Windows.Forms.Timer { Interval = 1200 };
                t.Tick += (ts, te) => {
                    btnAdd.Text      = "+ Add to Cart";
                    btnAdd.BackColor = ACCENT2;
                    btnAdd.ForeColor = Color.White;
                    t.Stop();
                };
                t.Start();
            };

            card.Controls.AddRange(new Control[] {
                lblCat, lblName, lblDesc, lblPrice, lblStock, btnAdd
            });
            return card;
        }

        private void UpdateCartButton()
        {
            btnCart.Text = $"🛒 Cart ({Session.CartCount})";
        }
    }
}
