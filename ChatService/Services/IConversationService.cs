using ChatService.DTOs;

namespace ChatService.Services;

public interface IConversationService
{
    Task<Conversation> CreateConvo(ConversationRequest conversationRequest);

    Task<ConversationsResponse> GetConversations(string username, string? continuationToken, int limit,
        long lastSeenConversationTime);


    Task<Conversation?> GetConversationById(string conversationId);

}