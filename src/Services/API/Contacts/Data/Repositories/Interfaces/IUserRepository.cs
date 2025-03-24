using API.Contacts.Model;

namespace API.Contacts.Data.Repositories;

/// <summary>
/// Repository interface for User entity
/// </summary>
public interface IUserRepository
{
    Task<User> GetByIdAsync(string id);
    Task<User> GetByOidcSubjectAsync(string oidcSubject);
    Task<IEnumerable<User>> GetUsersAsync(IEnumerable<string> userIds);
    Task<bool> AddAsync(User user);
    Task<bool> UpdateAsync(User user);
}
