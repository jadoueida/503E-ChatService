using System.Net.Mime;
using ChatService.DTOs;
using ChatService.Storage;

namespace ChatService.Services;

public class MessageService : IMessageService
{
    private readonly IMessageStore _messageStore;
    
    public MessageService(IMessageStore messageStore)
    {
        _messageStore = messageStore;
    }
    
    public Task<long> AddMessage(MessageRequest message, string conversationId)
    {
        return _messageStore.AddMessage(ToMessage(message,conversationId));
    }
    
    private Message ToMessage(MessageRequest message, string conversationId)
    {
        return new Message(
            MessageId: message.MessageId,
            ConversationId: conversationId,
            SenderUsername: message.SenderUsername,
            Text : message.Text,
            CreatedUnixTime: DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        );
    }
}