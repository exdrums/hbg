using API.Contacts.Domain.Models;
using System.Threading.Tasks;

namespace API.Contacts.Domain.Repositories
{
    /// <summary>
    /// Repository interface for User entities
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        /// <summary>
        /// Gets a user by OIDC subject
        /// </summary>
        Task<User> GetByOidcSubjectAsync(string oidcSubject);

        /// <summary>
        /// Gets or creates a user based on OIDC information
        /// </summary>
        Task<User> GetOrCreateFromOidcAsync(string oidcSubject, string displayName);

        /// <summary>
        /// Gets users by partial name match
        /// </summary>
        Task<IEnumerable<User>> SearchByNameAsync(string nameQuery, int limit = 20);

        /// <summary>
        /// Updates the last active timestamp for a user
        /// </summary>
        Task UpdateLastActiveAsync(string userId);
    }
}
