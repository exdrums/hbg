using API.Contacts.Model;
using Microsoft.EntityFrameworkCore;

namespace API.Contacts.Data.Repositories;

public class UserRepository : RepositoryBase<User>, IUserRepository
{
    public UserRepository(DbContext context) : base(context)
    {
    }

    public async Task<User> GetByIdAsync(string id)
    {
        return await FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User> GetByOidcSubjectAsync(string oidcSubject)
    {
        return await FirstOrDefaultAsync(u => u.OidcSubject == oidcSubject);
    }

    public async Task<IEnumerable<User>> GetUsersAsync(IEnumerable<string> userIds)
    {
        var idList = userIds.ToList();
        return await AsQueryable()
            .Where(u => idList.Contains(u.Id))
            .ToListAsync();
    }

    public async Task<bool> AddAsync(User user)
    {
        await AddAsync(user, default);
        return await SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(User user)
    {
        Update(user);
        return await SaveChangesAsync();
    }
}
