using ChatService.DTOs;
using ChatService.Storage;
using Microsoft.AspNetCore.Mvc;


namespace ChatService.Services;

public class ConversationService : IConversationService
{
    private readonly IConversationStore _conversationStore;
    private readonly IConversationParticipantStore _conversationParticipantStore;
    private readonly IMessageService _messageService;

    public ConversationService(IConversationStore conversationStore, IConversationParticipantStore conversationParticipantStore, IMessageService messageService)
    {
        _conversationStore = conversationStore;
        _conversationParticipantStore = conversationParticipantStore;
        _messageService = messageService;
    }

    public Task<Conversation> CreateConvo(ConversationRequest conversationRequest)
    {
        string conversationId = conversationRequest.Participants[0] + "_" + conversationRequest.Participants[1];
        long dateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        _conversationParticipantStore.AddParticipant(ToConvoParticipant(conversationId, conversationRequest.Participants[0]));
        _conversationParticipantStore.AddParticipant(ToConvoParticipant(conversationId, conversationRequest.Participants[1]));
        _messageService.AddMessage(conversationRequest.Message, conversationId, dateTime);
        Conversation conversation = ToConvo(conversationId, dateTime);
        _conversationStore.CreateConvo(conversation);
        return Task.FromResult<Conversation>(conversation);
    }

    public Task<List<Conversation>> GetConversations(string username, int offset, int limit,
        long lastSeenConversationTime)
    {
        return _conversationStore.GetConversations(username, offset, limit, lastSeenConversationTime);
    }

    // public Task<List<ConversationParticipant>> GetParticipantsByConversationId(string conversationId)
    // {
    //     return _conversationParticipantStore.GetParticipantsByConversationId(conversationId);
    // }


    private Conversation ToConvo(string conversationId, long dateTime)
    {
        return new Conversation(
            ConversationId: conversationId,
            ModifiedUnixTime: dateTime
        );
    }
    
    private ConversationParticipant ToConvoParticipant(string _conversationId, string username)
    {
        return new ConversationParticipant(
            conversationId: _conversationId,
            participantUsername: username
        );
    }
}

