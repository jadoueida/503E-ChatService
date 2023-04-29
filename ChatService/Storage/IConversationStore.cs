using ChatService.DTOs;

namespace ChatService.Storage;

public interface IConversationStore
{
    Task CreateConvo(Conversation conversation);
    
    //Task<Conversation?> GetConvoById(string convoId);
    
    //Task DeleteConvo(string imageId);
}