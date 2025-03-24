namespace API.Contacts.Model;

/// <summary>
/// Event fired when a user stopped typing
/// </summary>
public class UserStoppedTypingEvent : DomainEvent
{
    public string ConversationId { get; private set; }
    public string UserId { get; private set; }

    public UserStoppedTypingEvent(string conversationId, string userId)
    {
        ConversationId = conversationId;
        UserId = userId;
    }
}
