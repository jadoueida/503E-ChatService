using ChatService.DTOs;

namespace ChatService.Storage;

public interface IMessageStore
{
    Task<long> AddMessage(Message message);

    Task<(List<Message> Messages, string ContinuationToken)> GetConversationMessages(string conversationId, string? continuationToken,
        int limit, long lastSeenMessageTime);

    // Task<(List<Message> Messages, string ContinuationToken)> GetFirstConversationMessages(string conversationId, int limit,
    //     long lastSeenMessageTime);

    Task<Message?> GetMessage(string messageId);

    Task DeleteMessage(string messageId);

}