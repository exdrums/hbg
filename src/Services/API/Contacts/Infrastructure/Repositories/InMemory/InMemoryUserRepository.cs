using API.Contacts.Domain.Models;
using API.Contacts.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Contacts.Infrastructure.Repositories.InMemory
{
    /// <summary>
    /// In-memory implementation of the user repository
    /// </summary>
    public class InMemoryUserRepository : InMemoryRepositoryBase<User>, IUserRepository
    {
        private readonly Dictionary<string, string> _oidcSubjectToUserId = new();
        private readonly ILogger<InMemoryUserRepository> _logger;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public InMemoryUserRepository(ILogger<InMemoryUserRepository> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Add some sample data for testing
            SeedSampleData();
        }

        /// <summary>
        /// Gets the ID for a user
        /// </summary>
        protected override string GetId(User user)
        {
            return user?.Id;
        }

        /// <summary>
        /// Gets a user by OIDC subject
        /// </summary>
        public async Task<User> GetByOidcSubjectAsync(string oidcSubject)
        {
            if (string.IsNullOrEmpty(oidcSubject))
            {
                throw new ArgumentNullException(nameof(oidcSubject));
            }

            lock (_lock)
            {
                if (_oidcSubjectToUserId.TryGetValue(oidcSubject, out var userId))
                {
                    _entities.TryGetValue(userId, out var user);
                    return Task.FromResult(user);
                }

                return Task.FromResult<User>(null);
            }
        }

        /// <summary>
        /// Gets or creates a user based on OIDC information
        /// </summary>
        public async Task<User> GetOrCreateFromOidcAsync(string oidcSubject, string displayName)
        {
            if (string.IsNullOrEmpty(oidcSubject))
            {
                throw new ArgumentNullException(nameof(oidcSubject));
            }

            lock (_lock)
            {
                // Check if the user already exists
                if (_oidcSubjectToUserId.TryGetValue(oidcSubject, out var existingUserId))
                {
                    _entities.TryGetValue(existingUserId, out var existingUser);
                    if (existingUser != null)
                    {
                        // Update the display name if it has changed
                        if (!string.IsNullOrEmpty(displayName) && existingUser.Name != displayName)
                        {
                            existingUser.UpdateName(displayName);
                        }

                        // Update the last active timestamp
                        existingUser.UpdateLastActive();
                        return Task.FromResult(existingUser);
                    }
                }

                // Create a new user
                var newUser = User.CreateFromOidc(oidcSubject, displayName ?? "User");
                _entities[newUser.Id] = newUser;
                _oidcSubjectToUserId[oidcSubject] = newUser.Id;

                _logger.LogInformation("Created new user {UserId} from OIDC subject {OidcSubject}", newUser.Id, oidcSubject);
                return Task.FromResult(newUser);
            }
        }

        /// <summary>
        /// Gets users by partial name match
        /// </summary>
        public async Task<IEnumerable<User>> SearchByNameAsync(string nameQuery, int limit = 20)
        {
            if (string.IsNullOrEmpty(nameQuery))
            {
                return await Task.FromResult(Array.Empty<User>());
            }

            lock (_lock)
            {
                var query = nameQuery.ToLowerInvariant();
                return Task.FromResult(
                    _entities.Values
                        .Where(u => u.Name.ToLowerInvariant().Contains(query))
                        .OrderBy(u => u.Name)
                        .Take(limit)
                        .ToList()
                        .AsEnumerable()
                );
            }
        }

        /// <summary>
        /// Updates the last active timestamp for a user
        /// </summary>
        public async Task UpdateLastActiveAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            lock (_lock)
            {
                if (_entities.TryGetValue(userId, out var user))
                {
                    user.UpdateLastActive();
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Seeds some sample data for testing
        /// </summary>
        private void SeedSampleData()
        {
            // Create a system user
            var systemUser = new User("system", "system", "System");
            _entities[systemUser.Id] = systemUser;
            _oidcSubjectToUserId["system"] = systemUser.Id;

            // Create an AI assistant user
            var aiUser = new User("ai-assistant", "ai-assistant", "AI Assistant", "/images/ai-avatar.png", "AI Assistant Avatar");
            _entities[aiUser.Id] = aiUser;
            _oidcSubjectToUserId["ai-assistant"] = aiUser.Id;

            _logger.LogInformation("Seeded {Count} sample users", _entities.Count);
        }
    }
}
