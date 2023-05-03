using ChatService.DTOs;

namespace ChatService.Storage;

public interface IConversationStore
{
    Task CreateConvo(Conversation conversation);


    Task<List<Conversation>> GetConversations(string username, int offset, int limit,
        long lastSeenConversationTime);

    Task UpdateConversationTime(string conversationId, long dateTime);


    //Task<Conversation?> GetConvoById(string convoId);

    //Task DeleteConvo(string imageId);
}