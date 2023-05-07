using ChatService.DTOs;
using ChatService.Storage;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;


namespace ChatService.Services;

public class ConversationService : IConversationService
{
    private readonly IConversationStore _conversationStore;
    private readonly IMessageService _messageService;

    public ConversationService(IConversationStore conversationStore, IMessageService messageService)
    {
        _conversationStore = conversationStore;
        _messageService = messageService;
    }

    public Task<Conversation> CreateConvo(ConversationRequest conversationRequest)
    {
        string conversationId = conversationRequest.Participants[0] + "_" + conversationRequest.Participants[1];
        long dateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        _messageService.AddMessage(conversationRequest.Message, conversationId, dateTime);
        Conversation conversation = ToConvo(conversationId, conversationRequest.Participants[0],conversationRequest.Participants[1],dateTime);
        _conversationStore.CreateConvo(conversation);
        return Task.FromResult<Conversation>(conversation);
    }

    public Task<ConversationsResponse> GetConversations(string username, string? continuationToken, int limit,
        long lastSeenConversationTime)
    {
        if (continuationToken == null)
        {
            
            var resultTask = _conversationStore.GetFirstConversations(username, limit, lastSeenConversationTime);
            var result = resultTask.Result;
            return Task.FromResult(ToConversationsResponse(result,username, limit.ToString(), lastSeenConversationTime.ToString()));
        }
        else
        {
            string notNullContinuationToken = JsonConvert.DeserializeObject<string>(continuationToken);
            //string notNullContinuationToken = continuationToken;
            var resultTask =  _conversationStore.GetConversations(username, notNullContinuationToken, limit, lastSeenConversationTime);
            var result = resultTask.Result;
            return Task.FromResult(ToConversationsResponse(result,username, limit.ToString(), lastSeenConversationTime.ToString()));
        }
        
    }

    public Task<Conversation?> GetConversationById(string conversationId)
    {
        return _conversationStore.GetConversationById(conversationId);
    }


    private Conversation ToConvo(string conversationId,string username1, string username2 ,long dateTime)
    {
        return new Conversation(
            ConversationId: conversationId,
            Username1: username1,
            Username2: username2,
            ModifiedUnixTime: dateTime
        );
    }
    
    private ConversationsResponse ToConversationsResponse((List<Conversation> Conversations, string ContinuationToken) result, string username, string limit, string lastSeenConversationTime)
    {
        return new ConversationsResponse(
            Conversations:result.Conversations,
            NextUri: "/api/conversations?username={"+username+"}&limit={"+limit+"}&lastSeenConversationTime={"+lastSeenConversationTime+"}&continuationToken={"+result.ContinuationToken+"}"

        );
    }
}

