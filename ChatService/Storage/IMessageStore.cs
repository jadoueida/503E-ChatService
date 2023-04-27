using ChatService.DTOs;

namespace ChatService.Storage;

public interface IMessageStore
{
    Task<long> AddMessage(MessageRequest message);
    
}