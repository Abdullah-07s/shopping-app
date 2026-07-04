using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using ShoppingApp.Database;
using ShoppingApp.Models;

namespace ShoppingApp.Forms
{
    public class LoginForm : Form
    {
        // ── Controls ─────────────────────────────────────────────────────────
        private Panel pnlLeft, pnlRight;
        private Label lblBrand, lblTagline, lblTitle, lblSub;
        private Label lblUser, lblPass, lblError;
        private TextBox txtUsername, txtPassword;
        private Button btnLogin, btnGoRegister;
        private CheckBox chkShowPass;

        // ── Colours ───────────────────────────────────────────────────────────
        static readonly Color BG       = Color.FromArgb(15, 15, 25);
        static readonly Color CARD     = Color.FromArgb(25, 25, 40);
        static readonly Color ACCENT   = Color.FromArgb(99, 179, 237);
        static readonly Color ACCENT2  = Color.FromArgb(72, 149, 239);
        static readonly Color TEXT     = Color.FromArgb(240, 240, 255);
        static readonly Color MUTED    = Color.FromArgb(140, 140, 170);
        static readonly Color INPUT_BG = Color.FromArgb(35, 35, 55);
        static readonly Color ERR      = Color.FromArgb(252, 129, 129);

        public LoginForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text            = "ShopEase — Login";
            this.Size            = new Size(900, 580);
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox     = false;
            this.BackColor       = BG;

            // ── Left panel (branding) ─────────────────────────────────────────
            pnlLeft = new Panel
            {
                Size      = new Size(380, 580),
                Location  = new Point(0, 0),
                BackColor = Color.FromArgb(20, 20, 35)
            };

            lblBrand = new Label
            {
                Text      = "🛍️ ShopEase",
                Font      = new Font("Segoe UI", 26, FontStyle.Bold),
                ForeColor = ACCENT,
                AutoSize  = true,
                Location  = new Point(50, 180)
            };

            lblTagline = new Label
            {
                Text      = "Your one-stop shop for\neverything you need.",
                Font      = new Font("Segoe UI", 12),
                ForeColor = MUTED,
                AutoSize  = true,
                Location  = new Point(50, 250)
            };

            var lblFeatures = new Label
            {
                Text      = "✔  10,000+ products\n✔  Fast checkout\n✔  Secure payments\n✔  Order tracking",
                Font      = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(160, 200, 240),
                AutoSize  = true,
                Location  = new Point(50, 330)
            };

            pnlLeft.Controls.AddRange(new Control[] { lblBrand, lblTagline, lblFeatures });
            this.Controls.Add(pnlLeft);

            // ── Right panel (login card) ──────────────────────────────────────
            pnlRight = new Panel
            {
                Size      = new Size(520, 580),
                Location  = new Point(380, 0),
                BackColor = BG
            };

            lblTitle = new Label
            {
                Text      = "Welcome back",
                Font      = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = TEXT,
                AutoSize  = true,
                Location  = new Point(80, 100)
            };

            lblSub = new Label
            {
                Text      = "Sign in to your account",
                Font      = new Font("Segoe UI", 10),
                ForeColor = MUTED,
                AutoSize  = true,
                Location  = new Point(80, 148)
            };

            // Username
            lblUser = MakeLabel("Username", new Point(80, 210));
            txtUsername = MakeTextBox(new Point(80, 235), "Enter username");

            // Password
            lblPass = MakeLabel("Password", new Point(80, 295));
            txtPassword = MakeTextBox(new Point(80, 320), "Enter password", true);

            chkShowPass = new CheckBox
            {
                Text      = "Show password",
                ForeColor = MUTED,
                BackColor = Color.Transparent,
                Font      = new Font("Segoe UI", 9),
                Location  = new Point(80, 360),
                AutoSize  = true
            };
            chkShowPass.CheckedChanged += (s, e) =>
                txtPassword.PasswordChar = chkShowPass.Checked ? '\0' : '●';

            lblError = new Label
            {
                Text      = "",
                ForeColor = ERR,
                Font      = new Font("Segoe UI", 9),
                AutoSize  = true,
                Location  = new Point(80, 388)
            };

            btnLogin = MakeButton("Sign In", new Point(80, 410), ACCENT2);
            btnLogin.Click += BtnLogin_Click;

            var divider = new Label
            {
                Text      = "──────────────────────────",
                ForeColor = Color.FromArgb(50, 50, 80),
                AutoSize  = true,
                Location  = new Point(80, 465)
            };

            btnGoRegister = new Button
            {
                Text      = "Don't have an account?  Register →",
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10),
                ForeColor = ACCENT,
                BackColor = Color.Transparent,
                AutoSize  = true,
                Location  = new Point(80, 490),
                Cursor    = Cursors.Hand
            };
            btnGoRegister.FlatAppearance.BorderSize = 0;
            btnGoRegister.Click += (s, e) => {
                new RegisterForm().Show();
                this.Hide();
            };

            pnlRight.Controls.AddRange(new Control[] {
                lblTitle, lblSub,
                lblUser, txtUsername,
                lblPass, txtPassword,
                chkShowPass, lblError,
                btnLogin, divider, btnGoRegister
            });
            this.Controls.Add(pnlRight);
        }

        // ── Event handlers ────────────────────────────────────────────────────
        private void BtnLogin_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblError.Text = "⚠ Please fill in both fields.";
                return;
            }

            try
            {
                var dt = DBHelper.ExecuteQuery(
                    "SELECT * FROM Users WHERE Username=@u AND Password=@p",
                    new SqlParameter[] {
                        new SqlParameter("@u", username),
                        new SqlParameter("@p", password)
                    });

                if (dt.Rows.Count == 0)
                {
                    lblError.Text = "⚠ Incorrect username or password.";
                    return;
                }

                var row = dt.Rows[0];
                Session.CurrentUser = new User
                {
                    UserID   = (int)row["UserID"],
                    Username = row["Username"].ToString(),
                    FullName = row["FullName"].ToString(),
                    Email    = row["Email"].ToString(),
                    IsAdmin  = (bool)row["IsAdmin"]
                };

                new MainForm().Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                lblError.Text = "⚠ Database error: " + ex.Message;
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private Label MakeLabel(string text, Point loc) => new Label
        {
            Text      = text,
            Font      = new Font("Segoe UI", 9, FontStyle.Bold),
            ForeColor = MUTED,
            AutoSize  = true,
            Location  = loc
        };

        private TextBox MakeTextBox(Point loc, string placeholder, bool password = false)
        {
            var tb = new TextBox
            {
                Size            = new Size(360, 42),
                Location        = loc,
                BackColor       = INPUT_BG,
                ForeColor       = TEXT,
                Font            = new Font("Segoe UI", 11),
                BorderStyle     = BorderStyle.FixedSingle,
                PasswordChar    = password ? '●' : '\0'
            };
            return tb;
        }

        private Button MakeButton(string text, Point loc, Color color) => new Button
        {
            Text      = text,
            Size      = new Size(360, 45),
            Location  = loc,
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font("Segoe UI", 11, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
    }
}
