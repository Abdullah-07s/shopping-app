using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using ShoppingApp.Database;
using ShoppingApp.Models;

namespace ShoppingApp.Forms
{
    // ════════════════════════════════════════════════════════════════════════
    //  CHECKOUT FORM
    // ════════════════════════════════════════════════════════════════════════
    public class CheckoutForm : Form
    {
        static readonly Color BG     = Color.FromArgb(15, 15, 25);
        static readonly Color CARD   = Color.FromArgb(28, 28, 45);
        static readonly Color ACCENT = Color.FromArgb(99, 179, 237);
        static readonly Color ACCENT2= Color.FromArgb(72, 149, 239);
        static readonly Color GREEN  = Color.FromArgb(104, 211, 145);
        static readonly Color TEXT   = Color.FromArgb(240, 240, 255);
        static readonly Color MUTED  = Color.FromArgb(140, 140, 170);
        static readonly Color INPUT_BG = Color.FromArgb(35, 35, 55);

        private TextBox txtAddress;
        private Label lblError, lblTotal;
        private Button btnConfirm;

        public CheckoutForm()
        {
            this.Text            = "Checkout — ShopEase";
            this.Size            = new Size(560, 580);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.BackColor       = BG;

            int x = 40;

            AddLabel("📦 Order Summary", new Point(x, 30), 18, FontStyle.Bold, ACCENT);

            // Order items summary
            var pnlSummary = new Panel
            {
                Size      = new Size(480, 180),
                Location  = new Point(x, 75),
                BackColor = CARD,
                AutoScroll = true
            };

            int sy = 10;
            foreach (var item in Session.Cart)
            {
                var l = new Label
                {
                    Text      = $"{item.Product.Name}  ×{item.Quantity}   {item.SubtotalFormatted}",
                    Font      = new Font("Segoe UI", 10),
                    ForeColor = TEXT,
                    AutoSize  = true,
                    Location  = new Point(12, sy)
                };
                pnlSummary.Controls.Add(l);
                sy += 28;
            }
            this.Controls.Add(pnlSummary);

            var divider = new Panel
            {
                Size      = new Size(480, 1),
                Location  = new Point(x, 264),
                BackColor = Color.FromArgb(50, 50, 80)
            };
            this.Controls.Add(divider);

            lblTotal = new Label
            {
                Text      = $"Total:  Rs. {Session.CartTotal:N0}",
                Font      = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = GREEN,
                AutoSize  = true,
                Location  = new Point(x, 278)
            };
            this.Controls.Add(lblTotal);

            // Delivery address
            AddLabel("Delivery Address", new Point(x, 340), 10, FontStyle.Bold, MUTED);
            txtAddress = new TextBox
            {
                Size        = new Size(480, 80),
                Location    = new Point(x, 365),
                BackColor   = INPUT_BG,
                ForeColor   = TEXT,
                Font        = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                Multiline   = true,
                ScrollBars  = ScrollBars.Vertical
            };
            this.Controls.Add(txtAddress);

            lblError = new Label
            {
                Text      = "",
                ForeColor = Color.FromArgb(252, 129, 129),
                Font      = new Font("Segoe UI", 9),
                AutoSize  = true,
                Location  = new Point(x, 455)
            };
            this.Controls.Add(lblError);

            btnConfirm = new Button
            {
                Text      = "✔  Confirm Order",
                Size      = new Size(480, 48),
                Location  = new Point(x, 478),
                BackColor = ACCENT2,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnConfirm.FlatAppearance.BorderSize = 0;
            btnConfirm.Click += BtnConfirm_Click;
            this.Controls.Add(btnConfirm);
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                lblError.Text = "⚠ Please enter a delivery address.";
                return;
            }

            try
            {
                // Insert order
                var orderID = DBHelper.ExecuteScalar(
                    @"INSERT INTO Orders (UserID, TotalAmount, Address)
                      OUTPUT INSERTED.OrderID
                      VALUES (@uid, @total, @addr)",
                    new SqlParameter[] {
                        new SqlParameter("@uid",   Session.CurrentUser.UserID),
                        new SqlParameter("@total", Session.CartTotal),
                        new SqlParameter("@addr",  txtAddress.Text.Trim())
                    });

                int oid = Convert.ToInt32(orderID);

                // Insert order items + update stock
                foreach (var item in Session.Cart)
                {
                    DBHelper.ExecuteNonQuery(
                        @"INSERT INTO OrderItems (OrderID, ProductID, Quantity, UnitPrice)
                          VALUES (@oid, @pid, @qty, @price)",
                        new SqlParameter[] {
                            new SqlParameter("@oid",   oid),
                            new SqlParameter("@pid",   item.Product.ProductID),
                            new SqlParameter("@qty",   item.Quantity),
                            new SqlParameter("@price", item.Product.Price)
                        });

                    DBHelper.ExecuteNonQuery(
                        "UPDATE Products SET Stock = Stock - @qty WHERE ProductID = @pid",
                        new SqlParameter[] {
                            new SqlParameter("@qty", item.Quantity),
                            new SqlParameter("@pid", item.Product.ProductID)
                        });
                }

                Session.ClearCart();

                MessageBox.Show(
                    $"🎉 Order #{oid} placed successfully!\n\nYour items will be delivered to:\n{txtAddress.Text.Trim()}",
                    "Order Confirmed", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                lblError.Text = "⚠ Error: " + ex.Message;
            }
        }

        private void AddLabel(string text, Point loc, float size, FontStyle style, Color color)
        {
            this.Controls.Add(new Label {
                Text = text, Font = new Font("Segoe UI", size, style),
                ForeColor = color, AutoSize = true, Location = loc, BackColor = Color.Transparent
            });
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    //  ORDERS HISTORY FORM
    // ════════════════════════════════════════════════════════════════════════
    public class OrdersForm : Form
    {
        static readonly Color BG     = Color.FromArgb(15, 15, 25);
        static readonly Color CARD   = Color.FromArgb(28, 28, 45);
        static readonly Color ACCENT = Color.FromArgb(99, 179, 237);
        static readonly Color GREEN  = Color.FromArgb(104, 211, 145);
        static readonly Color TEXT   = Color.FromArgb(240, 240, 255);
        static readonly Color MUTED  = Color.FromArgb(140, 140, 170);

        public OrdersForm()
        {
            this.Text            = "My Orders — ShopEase";
            this.Size            = new Size(800, 600);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.BackColor       = BG;

            this.Controls.Add(new Label {
                Text = "📦 My Orders", Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = ACCENT, AutoSize = true, Location = new Point(30, 25)
            });

            var grid = new DataGridView
            {
                Size             = new Size(740, 480),
                Location         = new Point(30, 75),
                BackgroundColor  = CARD,
                ForeColor        = TEXT,
                GridColor        = Color.FromArgb(40, 40, 65),
                BorderStyle      = BorderStyle.None,
                ColumnHeadersDefaultCellStyle = { BackColor = Color.FromArgb(35, 35, 58),
                                                  ForeColor = ACCENT,
                                                  Font = new Font("Segoe UI", 10, FontStyle.Bold) },
                DefaultCellStyle = { BackColor = CARD, ForeColor = TEXT,
                                     Font = new Font("Segoe UI", 10),
                                     SelectionBackColor = Color.FromArgb(50, 80, 120) },
                RowHeadersVisible    = false,
                AllowUserToAddRows   = false,
                ReadOnly             = true,
                AutoSizeColumnsMode  = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode        = DataGridViewSelectionMode.FullRowSelect
            };

            this.Controls.Add(grid);

            try
            {
                var dt = DBHelper.ExecuteQuery(
                    @"SELECT o.OrderID as [Order #], 
                             CONVERT(NVARCHAR, o.OrderDate, 103) as [Date],
                             o.TotalAmount as [Total (Rs.)],
                             o.Status,
                             o.Address
                      FROM Orders o
                      WHERE o.UserID = @uid
                      ORDER BY o.OrderDate DESC",
                    new SqlParameter[] {
                        new SqlParameter("@uid", Session.CurrentUser.UserID)
                    });

                if (dt.Rows.Count == 0)
                {
                    grid.Visible = false;
                    this.Controls.Add(new Label {
                        Text = "You haven't placed any orders yet.",
                        Font = new Font("Segoe UI", 12), ForeColor = MUTED,
                        AutoSize = true, Location = new Point(30, 200)
                    });
                }
                else
                {
                    grid.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                grid.Visible = false;
                this.Controls.Add(new Label {
                    Text = "Error loading orders: " + ex.Message,
                    ForeColor = Color.FromArgb(252, 129, 129),
                    Font = new Font("Segoe UI", 10), AutoSize = true, Location = new Point(30, 200)
                });
            }
        }
    }
}
