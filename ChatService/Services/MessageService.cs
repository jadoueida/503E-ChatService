using System.Net.Mime;
using ChatService.DTOs;
using ChatService.Storage;

namespace ChatService.Services;

public class MessageService : IMessageService
{
    private readonly IMessageStore _messageStore;
    private readonly IConversationStore _conversationStore;
    
    public MessageService(IMessageStore messageStore, IConversationStore conversationStore)
    {
        _messageStore = messageStore;
        _conversationStore = conversationStore;
    }
    
    public Task<long> AddMessage(MessageRequest message, string conversationId, long dateTime = 0)
    {
        long defaultDateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (dateTime == 0)
        {
            _conversationStore.UpdateConversationTime(conversationId, defaultDateTime);
            return _messageStore.AddMessage(ToMessage(message,conversationId, defaultDateTime));
        }

        _conversationStore.UpdateConversationTime(conversationId, dateTime);
        return _messageStore.AddMessage(ToMessage(message,conversationId, dateTime));
    }

    public Task<List<Message>> GetConversationMessages(string conversationId, int offset, int limit, long lastSeenMessageTime)
    {
        return _messageStore.GetConversationMessages(conversationId, offset, limit, lastSeenMessageTime);
    }

    public Task<Message?> GetMessage(string messageId)
    {
        return _messageStore.GetMessage(messageId);
    }
    
    private Message ToMessage(MessageRequest message, string conversationId, long dateTime)
    {
        return new Message(
            MessageId: message.MessageId,
            ConversationId: conversationId,
            SenderUsername: message.SenderUsername,
            Text : message.Text,
            CreatedUnixTime: dateTime
        );
    }
}