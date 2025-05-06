using API.Contacts.Application.Dtos;
using System.Threading.Tasks;

namespace API.Contacts.Application.Interfaces
{
    /// <summary>
    /// Application service interface for user operations
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Gets a user by ID
        /// </summary>
        Task<UserDto> GetByIdAsync(string userId);

        /// <summary>
        /// Gets a user by OIDC subject
        /// </summary>
        Task<UserDto> GetByOidcSubjectAsync(string oidcSubject);

        /// <summary>
        /// Gets or creates a user based on OIDC information
        /// </summary>
        Task<UserDto> GetOrCreateUserFromOidcAsync(string oidcSubject, string displayName);

        /// <summary>
        /// Updates a user's profile information
        /// </summary>
        Task<UserDto> UpdateProfileAsync(string userId, string name, string avatarUrl = null, string avatarAlt = null);

        /// <summary>
        /// Searches for users by name
        /// </summary>
        Task<IEnumerable<UserDto>> SearchByNameAsync(string nameQuery, int limit = 20);

        /// <summary>
        /// Gets users by IDs
        /// </summary>
        Task<IEnumerable<UserDto>> GetByIdsAsync(IEnumerable<string> userIds);

        /// <summary>
        /// Updates the last active timestamp for a user
        /// </summary>
        Task UpdateLastActiveAsync(string userId);
    }
}
