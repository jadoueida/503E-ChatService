using ChatService.DTOs;

namespace ChatService.Storage;

public interface IConversationParticipantStore
{
    Task AddParticipant(ConversationParticipant conversationParticipant);
    
    // Task<List<ConversationParticipant>> GetParticipantsByConversationId(string converstaionId);
}