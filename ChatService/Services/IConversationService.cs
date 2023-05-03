using ChatService.DTOs;

namespace ChatService.Services;

public interface IConversationService
{
    Task<Conversation> CreateConvo(ConversationRequest conversationRequest);

    Task<List<Conversation>> GetConversations(string username, int offset, int limit,
        long lastSeenConversationTime);

    // Task<List<ConversationParticipant>> GetParticipantsByConversationId(string conversationId);

}