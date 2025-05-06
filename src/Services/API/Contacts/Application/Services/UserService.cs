using API.Contacts.Application.Dtos;
using API.Contacts.Application.Interfaces;
using API.Contacts.Domain.Models;
using API.Contacts.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Contacts.Application.Services
{
    /// <summary>
    /// Implementation of the user service for managing users
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets a user by ID
        /// </summary>
        public async Task<UserDto> GetByIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return null;
                }

                return MapToUserDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Gets a user by OIDC subject
        /// </summary>
        public async Task<UserDto> GetByOidcSubjectAsync(string oidcSubject)
        {
            if (string.IsNullOrEmpty(oidcSubject))
            {
                throw new ArgumentNullException(nameof(oidcSubject));
            }

            try
            {
                var user = await _userRepository.GetByOidcSubjectAsync(oidcSubject);
                if (user == null)
                {
                    return null;
                }

                return MapToUserDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by OIDC subject {OidcSubject}", oidcSubject);
                throw;
            }
        }

        /// <summary>
        /// Gets or creates a user based on OIDC information
        /// </summary>
        public async Task<UserDto> GetOrCreateUserFromOidcAsync(string oidcSubject, string displayName)
        {
            if (string.IsNullOrEmpty(oidcSubject))
            {
                throw new ArgumentNullException(nameof(oidcSubject));
            }

            try
            {
                var user = await _userRepository.GetOrCreateFromOidcAsync(oidcSubject, displayName ?? "User");
                return MapToUserDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting or creating user from OIDC {OidcSubject}", oidcSubject);
                throw;
            }
        }

        /// <summary>
        /// Updates a user's profile information
        /// </summary>
        public async Task<UserDto> UpdateProfileAsync(string userId, string name, string avatarUrl = null, string avatarAlt = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be empty", nameof(name));
            }

            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException($"User {userId} not found");
                }

                // Update name if it has changed
                if (user.Name != name)
                {
                    user.UpdateName(name);
                }

                // Update avatar if provided
                user.UpdateAvatar(avatarUrl, avatarAlt);

                // Save changes
                await _userRepository.UpdateAsync(user);

                _logger.LogInformation("Updated profile for user {UserId}", userId);

                return MapToUserDto(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Searches for users by name
        /// </summary>
        public async Task<IEnumerable<UserDto>> SearchByNameAsync(string nameQuery, int limit = 20)
        {
            if (string.IsNullOrEmpty(nameQuery))
            {
                return Enumerable.Empty<UserDto>();
            }

            try
            {
                var users = await _userRepository.SearchByNameAsync(nameQuery, limit);
                return users.Select(MapToUserDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users by name {NameQuery}", nameQuery);
                throw;
            }
        }

        /// <summary>
        /// Gets users by IDs
        /// </summary>
        public async Task<IEnumerable<UserDto>> GetByIdsAsync(IEnumerable<string> userIds)
        {
            if (userIds == null)
            {
                throw new ArgumentNullException(nameof(userIds));
            }

            try
            {
                var userDtos = new List<UserDto>();

                foreach (var userId in userIds)
                {
                    var user = await _userRepository.GetByIdAsync(userId);
                    if (user != null)
                    {
                        userDtos.Add(MapToUserDto(user));
                    }
                }

                return userDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by IDs");
                throw;
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

            try
            {
                await _userRepository.UpdateLastActiveAsync(userId);
                _logger.LogDebug("Updated last active timestamp for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last active timestamp for user {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Maps a User domain entity to a UserDto
        /// </summary>
        private UserDto MapToUserDto(User user)
        {
            if (user == null)
            {
                return null;
            }

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                AvatarUrl = user.AvatarUrl,
                AvatarAlt = user.AvatarAlt
            };
        }
    }
}
