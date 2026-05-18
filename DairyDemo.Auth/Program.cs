using DairyDemo.Auth.UI.Forms;
using DairyDemo.Auth.Data;

namespace DairyDemo.Auth;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        
        // Инициализация базы данных MySQL (создание таблицы и тестовых пользователей)
        try
        {
            Db.Initialize();
            Console.WriteLine("База данных MySQL успешно инициализирована.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Ошибка подключения к базе данных MySQL: {ex.Message}\n\n" +
                "Убедитесь, что:\n" +
                "1. MySQL сервер запущен\n" +
                "2. База данных 'dairy_auth' существует (или создана пользователем root)\n" +
                "3. Параметры подключения в Db.MySql.cs верны (логин, пароль, сервер)",
                "Ошибка подключения к БД",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            
            // Продолжаем работу с заглушкой (опционально)
            // return; // Раскомментируйте, чтобы остановить приложение при ошибке БД
        }
        
        Application.Run(new LoginForm());
    }
}
