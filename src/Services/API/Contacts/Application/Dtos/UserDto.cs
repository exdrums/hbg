namespace API.Contacts.Application.Dtos
{
    /// <summary>
    /// DTO for User entity
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// User identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// User display name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// URL to user's avatar image
        /// </summary>
        public string AvatarUrl { get; set; }

        /// <summary>
        /// Alt text for user's avatar
        /// </summary>
        public string AvatarAlt { get; set; }
    }
}
