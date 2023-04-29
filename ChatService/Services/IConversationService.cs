using ChatService.DTOs;

namespace ChatService.Services;

public interface IConversationService
{
    Task<Conversation> CreateConvo(ConversationRequest conversationRequest);
    
}