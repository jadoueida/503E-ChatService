using ChatService.DTOs;

namespace ChatService.Services;

public interface IMessageService
{
    Task<long> AddMessage(MessageRequest message, string conversationId, long dateTime = 0);
    
    Task<MessagesResponse> GetConversationMessages(string conversationId, string? continuationToken, int limit, long lastSeenMessageTime);

    Task<Message?> GetMessage(string messageId);

    Task DeleteMessage(string messageId);

}