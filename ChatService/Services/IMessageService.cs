using ChatService.DTOs;

namespace ChatService.Services;

public interface IMessageService
{
    Task<long> AddMessage(MessageRequest message, string conversationId, long dateTime = 0);
    
    Task<List<Message>> GetConversationMessages(string conversationId, int offset, int limit, long lastSeenMessageTime);

    Task<Message?> GetMessage(string messageId);

}