using MySql.Data.MySqlClient;
using DairyDemo.Auth.Data.Models;
using System.Collections.Concurrent;

namespace DairyDemo.Auth.Data;

public static class Db
{
    private static readonly string ConnectionString =
        "Server=localhost;Database=dairy_auth;Uid=Mark;Pwd=Password_Password;SslMode=none;";

    // Инициализация БД (создание таблицы и тестовых данных)
    public static void Initialize()
    {
        using var conn = new MySqlConnection(ConnectionString);
        conn.Open();

        var createTableCmd = @"
            CREATE TABLE IF NOT EXISTS users (
                Id INT AUTO_INCREMENT PRIMARY KEY,
                Login VARCHAR(50) NOT NULL UNIQUE,
                PasswordHash VARCHAR(255) NOT NULL,
                Role VARCHAR(20) NOT NULL,
                IsBlocked BOOLEAN DEFAULT FALSE,
                FailedAttempts INT DEFAULT 0
            );";

        using var cmd = new MySqlCommand(createTableCmd, conn);
        cmd.ExecuteNonQuery();

        // Проверка наличия администратора
        var checkAdminCmd = "SELECT COUNT(*) FROM users WHERE Login = 'admin';";
        using var checkCmd = new MySqlCommand(checkAdminCmd, conn);
        var count = Convert.ToInt32(checkCmd.ExecuteScalar());

        if (count == 0)
        {
            // Добавляем администратора
            var insertAdmin = @"
                INSERT INTO users (Login, PasswordHash, Role, IsBlocked, FailedAttempts) 
                VALUES ('admin', @hash, 'Admin', FALSE, 0);";
            using var insCmd = new MySqlCommand(insertAdmin, conn);
            insCmd.Parameters.AddWithValue("@hash", Services.PasswordService.HashPassword("admin"));
            insCmd.ExecuteNonQuery();

            // Добавляем тестового пользователя
            var insertUser = @"
                INSERT INTO users (Login, PasswordHash, Role, IsBlocked, FailedAttempts) 
                VALUES ('user', @hash, 'User', FALSE, 0);";
            using var insUserCmd = new MySqlCommand(insertUser, conn);
            insUserCmd.Parameters.AddWithValue("@hash", Services.PasswordService.HashPassword("user"));
            insUserCmd.ExecuteNonQuery();
        }
    }

    public static IEnumerable<User> GetAllUsers()
    {
        var users = new List<User>();
        using var conn = new MySqlConnection(ConnectionString);
        conn.Open();

        var cmd = new MySqlCommand("SELECT * FROM users;", conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            users.Add(new User
            {
                Id = reader.GetInt32("Id"),
                Login = reader.GetString("Login"),
                PasswordHash = reader.GetString("PasswordHash"),
                Role = reader.GetString("Role"),
                IsBlocked = reader.GetBoolean("IsBlocked"),
                FailedAttempts = reader.GetInt32("FailedAttempts")
            });
        }

        return users;
    }

    public static User? GetUserByLogin(string login)
    {
        using var conn = new MySqlConnection(ConnectionString);
        conn.Open();

        var cmd = new MySqlCommand("SELECT * FROM users WHERE Login = @login;", conn);
        cmd.Parameters.AddWithValue("@login", login);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new User
            {
                Id = reader.GetInt32("Id"),
                Login = reader.GetString("Login"),
                PasswordHash = reader.GetString("PasswordHash"),
                Role = reader.GetString("Role"),
                IsBlocked = reader.GetBoolean("IsBlocked"),
                FailedAttempts = reader.GetInt32("FailedAttempts")
            };
        }

        return null;
    }

    public static User? GetUserById(int id)
    {
        using var conn = new MySqlConnection(ConnectionString);
        conn.Open();

        var cmd = new MySqlCommand("SELECT * FROM users WHERE Id = @id;", conn);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new User
            {
                Id = reader.GetInt32("Id"),
                Login = reader.GetString("Login"),
                PasswordHash = reader.GetString("PasswordHash"),
                Role = reader.GetString("Role"),
                IsBlocked = reader.GetBoolean("IsBlocked"),
                FailedAttempts = reader.GetInt32("FailedAttempts")
            };
        }

        return null;
    }

    public static bool AddUser(User user)
    {
        // Проверка уникальности логина
        if (GetUserByLogin(user.Login) != null)
            return false;

        using var conn = new MySqlConnection(ConnectionString);
        conn.Open();

        var cmd = new MySqlCommand(@"
            INSERT INTO users (Login, PasswordHash, Role, IsBlocked, FailedAttempts) 
            VALUES (@login, @hash, @role, @blocked, @attempts);
            SELECT LAST_INSERT_ID();", conn);

        cmd.Parameters.AddWithValue("@login", user.Login);
        cmd.Parameters.AddWithValue("@hash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@role", user.Role);
        cmd.Parameters.AddWithValue("@blocked", user.IsBlocked);
        cmd.Parameters.AddWithValue("@attempts", user.FailedAttempts);

        var newId = Convert.ToInt32(cmd.ExecuteScalar());
        user.Id = newId;
        return true;
    }

    public static bool UpdateUser(User user)
    {
        // Проверка уникальности логина при изменении
        var existing = GetUserByLogin(user.Login);
        if (existing != null && existing.Id != user.Id)
            return false;

        using var conn = new MySqlConnection(ConnectionString);
        conn.Open();

        var cmd = new MySqlCommand(@"
            UPDATE users 
            SET Login = @login, PasswordHash = @hash, Role = @role, IsBlocked = @blocked, FailedAttempts = @attempts
            WHERE Id = @id;", conn);

        cmd.Parameters.AddWithValue("@login", user.Login);
        cmd.Parameters.AddWithValue("@hash", user.PasswordHash);
        cmd.Parameters.AddWithValue("@role", user.Role);
        cmd.Parameters.AddWithValue("@blocked", user.IsBlocked);
        cmd.Parameters.AddWithValue("@attempts", user.FailedAttempts);
        cmd.Parameters.AddWithValue("@id", user.Id);

        return cmd.ExecuteNonQuery() > 0;
    }

    public static void UnblockUser(int userId)
    {
        using var conn = new MySqlConnection(ConnectionString);
        conn.Open();

        var cmd = new MySqlCommand(@"
            UPDATE users 
            SET IsBlocked = FALSE, FailedAttempts = 0 
            WHERE Id = @id;", conn);

        cmd.Parameters.AddWithValue("@id", userId);
        cmd.ExecuteNonQuery();
    }

    public static void UpdateFailedAttempts(User user)
    {
        using var conn = new MySqlConnection(ConnectionString);
        conn.Open();

        var cmd = new MySqlCommand(@"
            UPDATE users 
            SET FailedAttempts = @attempts, IsBlocked = @blocked
            WHERE Id = @id;", conn);

        cmd.Parameters.AddWithValue("@attempts", user.FailedAttempts);
        cmd.Parameters.AddWithValue("@blocked", user.IsBlocked);
        cmd.Parameters.AddWithValue("@id", user.Id);

        cmd.ExecuteNonQuery();
    }
}
