namespace API.Contacts.Dtos;

/// <summary>
/// DTO for User entity
/// </summary>
public class UserDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string AvatarUrl { get; set; }
    public string AvatarAlt { get; set; }
}
