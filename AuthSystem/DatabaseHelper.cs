using System;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;

namespace AuthSystem
{
    public class DatabaseHelper
    {
        private static readonly string DbPath = "auth.db";
        private static readonly string ConnectionString = $"Data Source={DbPath};Version=3;";

        public static void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                
                // Создаем таблицу Пользователи если она не существует
                var createTableSql = @"
                    CREATE TABLE IF NOT EXISTS Пользователи (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Логин TEXT UNIQUE NOT NULL,
                        Пароль TEXT NOT NULL,
                        Роль TEXT NOT NULL DEFAULT 'Пользователь',
                        Заблокирован INTEGER DEFAULT 0,
                        НеудачныеПопытки INTEGER DEFAULT 0
                    )";
                
                using (var cmd = new SQLiteCommand(createTableSql, connection))
                {
                    cmd.ExecuteNonQuery();
                }

                // Добавляем администратора по умолчанию если его нет
                var checkAdminSql = "SELECT COUNT(*) FROM Пользователи WHERE Логин = 'admin'";
                using (var cmd = new SQLiteCommand(checkAdminSql, connection))
                {
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    if (count == 0)
                    {
                        string hashedPassword = BCrypt.Net.BCrypt.HashPassword("admin123");
                        var insertAdminSql = "INSERT INTO Пользователи (Логин, Пароль, Роль) VALUES ('admin', @password, 'Администратор')";
                        using (var insertCmd = new SQLiteCommand(insertAdminSql, connection))
                        {
                            insertCmd.Parameters.AddWithValue("@password", hashedPassword);
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public static bool ValidateUser(string login, string password)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                
                var sql = "SELECT Пароль, Роль, Заблокирован, НеудачныеПопытки FROM Пользователи WHERE Логин = @login";
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@login", login);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool isBlocked = Convert.ToBoolean(reader["Заблокирован"]);
                            if (isBlocked)
                            {
                                return false; // Пользователь заблокирован
                            }

                            string storedHash = reader["Пароль"].ToString();
                            string role = reader["Роль"].ToString();
                            int failedAttempts = Convert.ToInt32(reader["НеудачныеПопытки"]);

                            if (BCrypt.Net.BCrypt.Verify(password, storedHash))
                            {
                                // Сбрасываем счетчик неудачных попыток при успешном входе
                                ResetFailedAttempts(login);
                                return true;
                            }
                            else
                            {
                                // Увеличиваем счетчик неудачных попыток
                                IncrementFailedAttempts(login);
                                return false;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static bool IsUserBlocked(string login)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                var sql = "SELECT Заблокирован FROM Пользователи WHERE Логин = @login";
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@login", login);
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        return Convert.ToBoolean(result);
                    }
                }
            }
            return false;
        }

        public static void IncrementFailedAttempts(string login)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                
                // Получаем текущее количество попыток
                var selectSql = "SELECT НеудачныеПопытки FROM Пользователи WHERE Логин = @login";
                int failedAttempts = 0;
                using (var cmd = new SQLiteCommand(selectSql, connection))
                {
                    cmd.Parameters.AddWithValue("@login", login);
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        failedAttempts = Convert.ToInt32(result);
                    }
                }

                failedAttempts++;

                if (failedAttempts >= 3)
                {
                    // Блокируем пользователя
                    var blockSql = "UPDATE Пользователи SET Заблокирован = 1, НеудачныеПопытки = @attempts WHERE Логин = @login";
                    using (var cmd = new SQLiteCommand(blockSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@attempts", failedAttempts);
                        cmd.Parameters.AddWithValue("@login", login);
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    // Просто увеличиваем счетчик
                    var updateSql = "UPDATE Пользователи SET НеудачныеПопытки = @attempts WHERE Логин = @login";
                    using (var cmd = new SQLiteCommand(updateSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@attempts", failedAttempts);
                        cmd.Parameters.AddWithValue("@login", login);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public static void ResetFailedAttempts(string login)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                var sql = "UPDATE Пользователи SET НеудачныеПопытки = 0 WHERE Логин = @login";
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@login", login);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static bool UserExists(string login)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                var sql = "SELECT COUNT(*) FROM Пользователи WHERE Логин = @login";
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@login", login);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public static bool AddUser(string login, string password, string role)
        {
            if (UserExists(login))
            {
                return false;
            }

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                var sql = "INSERT INTO Пользователи (Логин, Пароль, Роль) VALUES (@login, @password, @role)";
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@login", login);
                    cmd.Parameters.AddWithValue("@password", hashedPassword);
                    cmd.Parameters.AddWithValue("@role", role);
                    cmd.ExecuteNonQuery();
                }
            }
            return true;
        }

        public static bool UpdateUser(string login, string newPassword, string newRole, bool unblock)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                
                var sql = "UPDATE Пользователи SET Роль = @role";
                if (!string.IsNullOrEmpty(newPassword))
                {
                    sql += ", Пароль = @password";
                }
                if (unblock)
                {
                    sql += ", Заблокирован = 0, НеудачныеПопытки = 0";
                }
                sql += " WHERE Логин = @login";

                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@role", newRole);
                    cmd.Parameters.AddWithValue("@login", login);
                    
                    if (!string.IsNullOrEmpty(newPassword))
                    {
                        cmd.Parameters.AddWithValue("@password", BCrypt.Net.BCrypt.HashPassword(newPassword));
                    }
                    
                    cmd.ExecuteNonQuery();
                }
            }
            return true;
        }

        public static SQLiteDataReader GetAllUsers()
        {
            var connection = new SQLiteConnection(ConnectionString);
            connection.Open();
            var sql = "SELECT Id, Логин, Роль, Заблокирован FROM Пользователи";
            var cmd = new SQLiteCommand(sql, connection);
            return cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
        }

        public static string GetUserRole(string login)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                var sql = "SELECT Роль FROM Пользователи WHERE Логин = @login";
                using (var cmd = new SQLiteCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@login", login);
                    var result = cmd.ExecuteScalar();
                    return result?.ToString() ?? "";
                }
            }
        }
    }
}
