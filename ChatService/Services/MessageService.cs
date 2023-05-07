using System.Net;
using System.Net.Mime;
using System.Web;
using ChatService.DTOs;
using ChatService.Storage;
using Newtonsoft.Json;

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

    public Task<MessagesResponse> GetConversationMessages(string conversationId, string? continuationToken, int limit, long lastSeenMessageTime)
    {
        if (continuationToken == null)
        {
            
            var resultTask = _messageStore.GetFirstConversationMessages(conversationId,limit,lastSeenMessageTime);
            var result = resultTask.Result;
            return Task.FromResult(ToMessagesResponse(result, conversationId, limit.ToString(), lastSeenMessageTime.ToString() ));
        }
        else
        {
            string notNullContinuationToken = JsonConvert.DeserializeObject<string>(continuationToken);
            //string notNullContinuationToken = continuationToken;
            var resultTask =  _messageStore.GetConversationMessages(conversationId,notNullContinuationToken,limit,lastSeenMessageTime);
            var result = resultTask.Result;
            return Task.FromResult(ToMessagesResponse(result, conversationId, limit.ToString(), lastSeenMessageTime.ToString() ));
        }
    }

    public Task<Message?> GetMessage(string messageId)
    {
        return _messageStore.GetMessage(messageId);
    }

    private MessagesResponse ToMessagesResponse((List<Message> Messages, string ContinuationToken) result, string conversationId, string limit, string lastSeenMessageTime)
    {
        return new MessagesResponse(
            Messages:result.Messages,
            NextUri: WebUtility.UrlEncode("/api/conversations/{" + conversationId + "}/messages?&limit={" + limit + "}&lastSeenMessageTime={" + lastSeenMessageTime + "}&continuationToken={" + result.ContinuationToken + "}")
        );
    }
    
    private Message ToMessage(MessageRequest message, string conversationId, long dateTime)
    {
        return new Message(
            id: message.MessageId,
            ConversationId: conversationId,
            SenderUsername: message.SenderUsername,
            Text : message.Text,
            CreatedUnixTime: dateTime
        );
    }
}