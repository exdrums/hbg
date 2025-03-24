namespace API.Contacts.Model;

/// <summary>
/// Event fired when a user started typing
/// </summary>
public class UserStartedTypingEvent : DomainEvent
{
    public string ConversationId { get; private set; }
    public string UserId { get; private set; }

    public UserStartedTypingEvent(string conversationId, string userId)
    {
        ConversationId = conversationId;
        UserId = userId;
    }
}
