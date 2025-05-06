using System;

namespace API.Contacts.Domain.Models
{
    /// <summary>
    /// Represents a user in the chat system
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique identifier for the user
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// External OIDC subject identifier
        /// </summary>
        public string OidcSubject { get; private set; }

        /// <summary>
        /// Display name for the user
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// URL to the user's avatar image
        /// </summary>
        public string AvatarUrl { get; private set; }

        /// <summary>
        /// Alt text for the user's avatar image
        /// </summary>
        public string AvatarAlt { get; private set; }

        /// <summary>
        /// Date and time when the user was created
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Date and time when the user was last active
        /// </summary>
        public DateTime LastActiveAt { get; private set; }

        /// <summary>
        /// Private constructor for EF Core
        /// </summary>
        private User() { }

        /// <summary>
        /// Creates a new user with the specified properties
        /// </summary>
        public User(string id, string oidcSubject, string name, string avatarUrl = null, string avatarAlt = null)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            OidcSubject = oidcSubject ?? throw new ArgumentNullException(nameof(oidcSubject));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            AvatarUrl = avatarUrl;
            AvatarAlt = avatarAlt;
            CreatedAt = DateTime.UtcNow;
            LastActiveAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a new user from OIDC information
        /// </summary>
        public static User CreateFromOidc(string oidcSubject, string displayName)
        {
            if (string.IsNullOrEmpty(oidcSubject)) throw new ArgumentNullException(nameof(oidcSubject));
            if (string.IsNullOrEmpty(displayName)) throw new ArgumentNullException(nameof(displayName));

            string id = Guid.NewGuid().ToString();
            return new User(id, oidcSubject, displayName);
        }

        /// <summary>
        /// Updates the user's display name
        /// </summary>
        public void UpdateName(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            Name = name;
        }

        /// <summary>
        /// Updates the user's avatar
        /// </summary>
        public void UpdateAvatar(string avatarUrl, string avatarAlt)
        {
            AvatarUrl = avatarUrl;
            AvatarAlt = avatarAlt;
        }

        /// <summary>
        /// Updates the user's last active timestamp
        /// </summary>
        public void UpdateLastActive()
        {
            LastActiveAt = DateTime.UtcNow;
        }
    }
}
