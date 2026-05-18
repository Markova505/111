using DairyDemo.Auth.Data.Models;

namespace DairyDemo.Auth.Data.Repositories;

public class UserRepository
{
    public IEnumerable<User> GetAllUsers() => Db.GetAllUsers();

    public User? GetUserByLogin(string login) => Db.GetUserByLogin(login);

    public User? GetUserById(int id) => Db.GetUserById(id);

    public bool AddUser(User user) => Db.AddUser(user);

    public bool UpdateUser(User user) => Db.UpdateUser(user);

    public void UnblockUser(int userId) => Db.UnblockUser(userId);
}
