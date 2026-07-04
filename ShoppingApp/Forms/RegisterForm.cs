using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using ShoppingApp.Database;
using ShoppingApp.Models;

namespace ShoppingApp.Forms
{
    public class RegisterForm : Form
    {
        static readonly Color BG       = Color.FromArgb(15, 15, 25);
        static readonly Color ACCENT   = Color.FromArgb(99, 179, 237);
        static readonly Color ACCENT2  = Color.FromArgb(72, 149, 239);
        static readonly Color TEXT     = Color.FromArgb(240, 240, 255);
        static readonly Color MUTED    = Color.FromArgb(140, 140, 170);
        static readonly Color INPUT_BG = Color.FromArgb(35, 35, 55);
        static readonly Color ERR      = Color.FromArgb(252, 129, 129);
        static readonly Color SUCCESS  = Color.FromArgb(104, 211, 145);

        private TextBox txtUsername, txtFullName, txtEmail, txtPassword, txtConfirm;
        private Label lblError;
        private Button btnRegister, btnBack;

        public RegisterForm()
        {
            this.Text            = "ShopEase — Register";
            this.Size            = new Size(520, 620);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.BackColor       = BG;

            int x = 80, y = 60, gap = 70;

            AddLabel("🛍️ Create your account", new Point(x, y), 18, FontStyle.Bold, ACCENT);
            AddLabel("Join ShopEase today — it's free.", new Point(x, y + 40), 10, FontStyle.Regular, MUTED);

            y += 110;
            AddFieldLabel("Full Name",  new Point(x, y));        txtFullName  = AddTextBox(new Point(x, y + 22));
            y += gap;
            AddFieldLabel("Username",   new Point(x, y));        txtUsername  = AddTextBox(new Point(x, y + 22));
            y += gap;
            AddFieldLabel("Email",      new Point(x, y));        txtEmail     = AddTextBox(new Point(x, y + 22));
            y += gap;
            AddFieldLabel("Password",   new Point(x, y));        txtPassword  = AddTextBox(new Point(x, y + 22), true);
            y += gap;
            AddFieldLabel("Confirm Password", new Point(x, y)); txtConfirm   = AddTextBox(new Point(x, y + 22), true);
            y += gap;

            lblError = new Label { Text = "", ForeColor = ERR,
                Font = new Font("Segoe UI", 9), AutoSize = true, Location = new Point(x, y) };
            this.Controls.Add(lblError);

            y += 24;
            btnRegister = MakeButton("Create Account", new Point(x, y), ACCENT2);
            btnRegister.Click += BtnRegister_Click;

            btnBack = new Button
            {
                Text = "← Back to Login", FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10), ForeColor = ACCENT,
                BackColor = Color.Transparent, AutoSize = true,
                Location = new Point(x, y + 55), Cursor = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += (s, e) => { new LoginForm().Show(); this.Close(); };

            this.Controls.AddRange(new Control[] { btnRegister, btnBack });
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            if (string.IsNullOrWhiteSpace(txtFullName.Text) ||
                string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                lblError.Text = "⚠ Full name, username, and password are required.";
                return;
            }
            if (txtPassword.Text != txtConfirm.Text)
            {
                lblError.Text = "⚠ Passwords do not match.";
                return;
            }
            if (txtPassword.Text.Length < 6)
            {
                lblError.Text = "⚠ Password must be at least 6 characters.";
                return;
            }

            try
            {
                var check = DBHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM Users WHERE Username=@u",
                    new SqlParameter[] { new SqlParameter("@u", txtUsername.Text.Trim()) });

                if (Convert.ToInt32(check) > 0)
                {
                    lblError.Text = "⚠ Username already taken.";
                    return;
                }

                DBHelper.ExecuteNonQuery(
                    @"INSERT INTO Users (Username, Password, FullName, Email)
                      VALUES (@u, @p, @f, @e)",
                    new SqlParameter[] {
                        new SqlParameter("@u", txtUsername.Text.Trim()),
                        new SqlParameter("@p", txtPassword.Text),
                        new SqlParameter("@f", txtFullName.Text.Trim()),
                        new SqlParameter("@e", txtEmail.Text.Trim())
                    });

                lblError.ForeColor = SUCCESS;
                lblError.Text      = "✔ Account created! Redirecting to login...";
                System.Threading.Thread.Sleep(1200);
                new LoginForm().Show();
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

        private void AddFieldLabel(string text, Point loc)
        {
            this.Controls.Add(new Label {
                Text = text, Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = MUTED, AutoSize = true, Location = loc
            });
        }

        private TextBox AddTextBox(Point loc, bool password = false)
        {
            var tb = new TextBox {
                Size = new Size(360, 38), Location = loc,
                BackColor = INPUT_BG, ForeColor = TEXT,
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                PasswordChar = password ? '●' : '\0'
            };
            this.Controls.Add(tb);
            return tb;
        }

        private Button MakeButton(string text, Point loc, Color color)
        {
            var btn = new Button {
                Text = text, Size = new Size(360, 45), Location = loc,
                BackColor = color, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand
            };
            this.Controls.Add(btn);
            return btn;
        }
    }
}
