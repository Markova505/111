using System.Collections.Concurrent;
using DairyDemo.Auth.Data.Models;

namespace DairyDemo.Auth.Data;

public static class Db
{
    private static readonly ConcurrentDictionary<int, User> _users = new();
    private static int _nextId = 1;

    static Db()
    {
        // Добавляем администратора по умолчанию
        var admin = new User
        {
            Id = _nextId++,
            Login = "admin",
            PasswordHash = PasswordService.HashPassword("admin"),
            Role = "Admin"
        };
        _users[admin.Id] = admin;

        // Добавляем тестового пользователя
        var user = new User
        {
            Id = _nextId++,
            Login = "user",
            PasswordHash = PasswordService.HashPassword("user"),
            Role = "User"
        };
        _users[user.Id] = user;
    }

    public static IEnumerable<User> GetAllUsers() => _users.Values.ToList();

    public static User? GetUserByLogin(string login) => 
        _users.Values.FirstOrDefault(u => u.Login == login);

    public static User? GetUserById(int id) => 
        _users.TryGetValue(id, out var user) ? user : null;

    public static bool AddUser(User user)
    {
        if (_users.Values.Any(u => u.Login == user.Login))
            return false;

        user.Id = _nextId++;
        _users[user.Id] = user;
        return true;
    }

    public static bool UpdateUser(User user)
    {
        if (!_users.ContainsKey(user.Id))
            return false;

        // Проверка уникальности логина при изменении
        if (_users.Values.Any(u => u.Login == user.Login && u.Id != user.Id))
            return false;

        _users[user.Id] = user;
        return true;
    }

    public static void UnblockUser(int userId)
    {
        if (_users.TryGetValue(userId, out var user))
        {
            user.IsBlocked = false;
            user.FailedAttempts = 0;
        }
    }
}
