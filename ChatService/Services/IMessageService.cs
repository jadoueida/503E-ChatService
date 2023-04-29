using ChatService.DTOs;

namespace ChatService.Services;

public interface IMessageService
{
    Task<long> AddMessage(MessageRequest message, string conversationId);

}