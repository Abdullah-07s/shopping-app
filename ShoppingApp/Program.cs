using System;
using System.Windows.Forms;
using ShoppingApp.Database;
using ShoppingApp.Forms;

namespace ShoppingApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Quick DB connection check
            if (!DBHelper.TestConnection())
            {
                var result = MessageBox.Show(
                    "Could not connect to SQL Server with the default settings.\n\n" +
                    "Default: Server=.\\SQLEXPRESS, Database=ShoppingAppDB\n\n" +
                    "Make sure SQL Server Express is running.\n" +
                    "See README.md for setup instructions.\n\n" +
                    "Click OK to continue anyway, or Cancel to exit.",
                    "Database Connection",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Cancel) return;
            }
            else
            {
                // Auto-create tables and seed data on first run
                try { DBHelper.InitializeDatabase(); }
                catch { /* tables may already exist */ }
            }

            Application.Run(new LoginForm());
        }
    }
}
