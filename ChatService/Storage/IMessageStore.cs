using ChatService.DTOs;

namespace ChatService.Storage;

public interface IMessageStore
{
    Task<long> AddMessage(Message message);

    Task<List<Message>> GetConversationMessages(string conversationId, int offset, int limit,long lastSeenMessageTime);

    Task<Message?> GetMessage(string messageId);

}