namespace API.Contacts.Model;

/// <summary>
/// Event fired when a message is sent
/// </summary>
public class MessageSentEvent : DomainEvent
{
    public string MessageId { get; private set; }
    public string ConversationId { get; private set; }
    public string UserId { get; private set; }

    public MessageSentEvent(string messageId, string conversationId, string userId)
    {
        MessageId = messageId;
        ConversationId = conversationId;
        UserId = userId;
    }
}
