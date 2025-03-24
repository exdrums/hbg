using API.Contacts.Model;

namespace API.Contacts.Dtos;

/// <summary>
/// DTO for Conversation entity
/// </summary>
public class ConversationDto
{
    public string Id { get; set; }
    public string Title { get; set; }
    public ConversationType Type { get; set; }
    public List<UserDto> Participants { get; set; }
    public DateTime LastMessageAt { get; set; }
    public bool IsArchived { get; set; }
}
