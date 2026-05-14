 using System;
using System.Windows.Forms;

namespace AuthSystem
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);
            
            // Инициализация базы данных
            DatabaseHelper.InitializeDatabase();
            
            Application.Run(new LoginForm());
        }
    }
}
