using DairyDemo.Auth.Data;
using DairyDemo.Auth.Data.Models;

namespace DairyDemo.Auth.Services;

public class AuthService
{
    private const int MaxFailedAttempts = 3;

    public AuthResult Authenticate(string login, string password)
    {
        var user = Db.GetUserByLogin(login);

        if (user == null)
            return new AuthResult(false, "Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные");

        if (user.IsBlocked)
            return new AuthResult(false, "Вы заблокированы. Обратитесь к администратору");

        if (!PasswordService.VerifyPassword(password, user.PasswordHash))
        {
            user.FailedAttempts++;
            if (user.FailedAttempts >= MaxFailedAttempts)
            {
                user.IsBlocked = true;
                return new AuthResult(false, "Вы заблокированы. Обратитесь к администратору");
            }
            return new AuthResult(false, "Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные");
        }

        user.FailedAttempts = 0;
        return new AuthResult(true, "Вы успешно авторизовались", user);
    }

    public bool IsCaptchaValid(List<int> userOrder, List<int> correctOrder)
    {
        if (userOrder.Count != correctOrder.Count)
            return false;

        for (int i = 0; i < userOrder.Count; i++)
        {
            if (userOrder[i] != correctOrder[i])
                return false;
        }

        return true;
    }
}

public class AuthResult
{
    public bool Success { get; }
    public string Message { get; }
    public User? User { get; }

    public AuthResult(bool success, string message, User? user = null)
    {
        Success = success;
        Message = message;
        User = user;
    }
}
