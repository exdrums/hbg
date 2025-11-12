using System;

namespace API.Constructor.Models.DTOs
{
    public class ChatMessageDto
    {
        public Guid InteractionId { get; set; }
        public Guid ProjectId { get; set; }
        public string UserMessage { get; set; }
        public string AssistantResponse { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SendChatMessageDto
    {
        public Guid ProjectId { get; set; }
        public string Message { get; set; }
    }
}
