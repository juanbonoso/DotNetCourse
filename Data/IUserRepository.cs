using DotNetApi.Models;

namespace DotNetApi.Data;

public interface IUserRepository
{
    public Task<bool> SaveChangesAsync();
    public void AddEntity<T>(T entityToAdd);
    public void RemoveEntity<T>(T entityToRemove);
    public IEnumerable<User> GetUsers();
    public User GetSingleUser(int userId);
    public UserSalary GetSingleUserSalary(int userId);
    public UserJobInfo GetSingleUserJobInfo(int userId);
}