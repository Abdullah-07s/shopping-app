using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using ShoppingApp.Database;
using ShoppingApp.Models;

namespace ShoppingApp.Forms
{
    public class CartForm : Form
    {
        static readonly Color BG      = Color.FromArgb(15, 15, 25);
        static readonly Color CARD    = Color.FromArgb(28, 28, 45);
        static readonly Color ACCENT  = Color.FromArgb(99, 179, 237);
        static readonly Color ACCENT2 = Color.FromArgb(72, 149, 239);
        static readonly Color GREEN   = Color.FromArgb(104, 211, 145);
        static readonly Color TEXT    = Color.FromArgb(240, 240, 255);
        static readonly Color MUTED   = Color.FromArgb(140, 140, 170);
        static readonly Color RED     = Color.FromArgb(252, 129, 129);

        private Panel pnlItems;
        private Label lblTotal, lblEmpty;
        private Button btnCheckout, btnClear;

        public CartForm()
        {
            this.Text            = "Your Cart — ShopEase";
            this.Size            = new Size(700, 600);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.BackColor       = BG;

            var lblTitle = new Label
            {
                Text      = "🛒 Your Cart",
                Font      = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = ACCENT,
                AutoSize  = true,
                Location  = new Point(30, 25)
            };
            this.Controls.Add(lblTitle);

            // Scrollable items panel
            pnlItems = new Panel
            {
                Size        = new Size(640, 380),
                Location    = new Point(30, 70),
                BackColor   = BG,
                AutoScroll  = true
            };
            this.Controls.Add(pnlItems);

            // Divider
            var div = new Panel
            {
                Size      = new Size(640, 1),
                Location  = new Point(30, 460),
                BackColor = Color.FromArgb(50, 50, 80)
            };
            this.Controls.Add(div);

            // Total
            lblTotal = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = GREEN,
                AutoSize  = true,
                Location  = new Point(30, 473)
            };
            this.Controls.Add(lblTotal);

            // Buttons
            btnCheckout = new Button
            {
                Text      = "Proceed to Checkout →",
                Size      = new Size(220, 44),
                Location  = new Point(420, 468),
                BackColor = ACCENT2,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnCheckout.FlatAppearance.BorderSize = 0;
            btnCheckout.Click += BtnCheckout_Click;
            this.Controls.Add(btnCheckout);

            btnClear = new Button
            {
                Text      = "Clear Cart",
                Size      = new Size(120, 44),
                Location  = new Point(290, 468),
                BackColor = Color.FromArgb(60, 25, 25),
                ForeColor = RED,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10),
                Cursor    = Cursors.Hand
            };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += (s, e) => { Session.ClearCart(); RenderCart(); };
            this.Controls.Add(btnClear);

            RenderCart();
        }

        private void RenderCart()
        {
            pnlItems.Controls.Clear();

            if (Session.Cart.Count == 0)
            {
                pnlItems.Controls.Add(new Label
                {
                    Text      = "Your cart is empty.\nBrowse products to add items.",
                    Font      = new Font("Segoe UI", 12),
                    ForeColor = MUTED,
                    AutoSize  = true,
                    Location  = new Point(20, 80)
                });
                lblTotal.Text         = "Total: Rs. 0";
                btnCheckout.Enabled   = false;
                btnCheckout.BackColor = Color.FromArgb(40, 40, 60);
                return;
            }

            btnCheckout.Enabled   = true;
            btnCheckout.BackColor = ACCENT2;

            int y = 5;
            foreach (var item in Session.Cart)
            {
                var row = new Panel
                {
                    Size      = new Size(620, 75),
                    Location  = new Point(0, y),
                    BackColor = CARD
                };

                var lblName = new Label
                {
                    Text      = item.Product.Name,
                    Font      = new Font("Segoe UI", 11, FontStyle.Bold),
                    ForeColor = TEXT,
                    AutoSize  = true,
                    Location  = new Point(15, 12)
                };

                var lblPrice = new Label
                {
                    Text      = item.Product.PriceFormatted + " each",
                    Font      = new Font("Segoe UI", 9),
                    ForeColor = MUTED,
                    AutoSize  = true,
                    Location  = new Point(15, 38)
                };

                // Quantity controls
                var btnMinus = MakeSmallBtn("−", new Point(360, 22), Color.FromArgb(50, 50, 80));
                var lblQty   = new Label
                {
                    Text      = item.Quantity.ToString(),
                    Font      = new Font("Segoe UI", 11, FontStyle.Bold),
                    ForeColor = TEXT,
                    Size      = new Size(30, 28),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location  = new Point(398, 22)
                };
                var btnPlus = MakeSmallBtn("+", new Point(432, 22), ACCENT2);

                var lblSub = new Label
                {
                    Text      = item.SubtotalFormatted,
                    Font      = new Font("Segoe UI", 11, FontStyle.Bold),
                    ForeColor = GREEN,
                    AutoSize  = true,
                    Location  = new Point(490, 27)
                };

                var btnRemove = new Button
                {
                    Text      = "✕",
                    Size      = new Size(28, 28),
                    Location  = new Point(585, 22),
                    BackColor = Color.FromArgb(80, 25, 25),
                    ForeColor = RED,
                    FlatStyle = FlatStyle.Flat,
                    Cursor    = Cursors.Hand
                };
                btnRemove.FlatAppearance.BorderSize = 0;

                var capturedItem = item;
                btnMinus.Click += (s, e) => {
                    if (capturedItem.Quantity > 1) capturedItem.Quantity--;
                    else Session.RemoveFromCart(capturedItem.Product.ProductID);
                    RenderCart();
                };
                btnPlus.Click += (s, e) => {
                    capturedItem.Quantity++;
                    RenderCart();
                };
                btnRemove.Click += (s, e) => {
                    Session.RemoveFromCart(capturedItem.Product.ProductID);
                    RenderCart();
                };

                row.Controls.AddRange(new Control[] {
                    lblName, lblPrice, btnMinus, lblQty, btnPlus, lblSub, btnRemove
                });
                pnlItems.Controls.Add(row);
                y += 82;
            }

            lblTotal.Text = $"Total:  Rs. {Session.CartTotal:N0}";
        }

        private Button MakeSmallBtn(string text, Point loc, Color color)
        {
            var b = new Button
            {
                Text      = text,
                Size      = new Size(32, 28),
                Location  = loc,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private void BtnCheckout_Click(object sender, EventArgs e)
        {
            new CheckoutForm().ShowDialog();
            RenderCart();
        }
    }
}
