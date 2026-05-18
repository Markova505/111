namespace DairyDemo.Auth.Data.Models;

public class User
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User"; // "Admin" or "User"
    public bool IsBlocked { get; set; } = false;
    public int FailedAttempts { get; set; } = 0;
}
