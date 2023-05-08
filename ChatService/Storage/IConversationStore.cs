using ChatService.DTOs;

namespace ChatService.Storage;

public interface IConversationStore
{
    Task CreateConvo(Conversation conversation);


    Task<(List<Conversation> Conversations, string ContinuationToken)> GetConversations(string username, string? continuationToken, int limit,
        long lastSeenConversationTime);
    
    // Task<(List<Conversation> Conversations, string ContinuationToken)> GetFirstConversations(string username, int limit, long lastSeenConversationTime);

    Task UpdateConversationTime(string conversationId, long dateTime);


    Task<Conversation?> GetConversationById(string conversationId);

    //Task DeleteConvo(string imageId);
}