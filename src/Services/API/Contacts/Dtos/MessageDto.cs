namespace API.Contacts.Dtos;

/// <summary>
/// DTO for Message entity
/// </summary>
public class MessageDto
{
    public string Id { get; set; }
    public string Text { get; set; }
    public DateTime Timestamp { get; set; }
    public UserDto Author { get; set; }
    public bool IsEdited { get; set; }
    public bool IsBeingRegenerated { get; set; }
    public string ParentMessageId { get; set; }
}
