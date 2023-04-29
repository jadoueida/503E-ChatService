using ChatService.DTOs;
using ChatService.Storage;
using Microsoft.AspNetCore.Mvc;


namespace ChatService.Services;

public class ConversationService : IConversationService
{
    private readonly IConversationStore _conversationStore;
    private readonly IConversationParticipantStore _conversationParticipant;
    private readonly IMessageService _messageService;

    public ConversationService(IConversationStore conversationStore, IConversationParticipantStore conversationParticipant, IMessageService messageService)
    {
        _conversationStore = conversationStore;
        _conversationParticipant = conversationParticipant;
        _messageService = messageService;
    }

    public Task<Conversation> CreateConvo(ConversationRequest conversationRequest)
    {
        string conversationId = Guid.NewGuid().ToString();
        _conversationParticipant.AddParticipant(ToConvoParticipant(conversationId, conversationRequest.Participants[0]));
        _conversationParticipant.AddParticipant(ToConvoParticipant(conversationId, conversationRequest.Participants[1]));
        _messageService.AddMessage(conversationRequest.Message, conversationId);
        Conversation conversation = ToConvo(conversationId);
        _conversationStore.CreateConvo(conversation);
        return Task.FromResult<Conversation>(conversation);
    }
    
    private Conversation ToConvo(string conversationId)
    {
        return new Conversation(
            ConversationId: conversationId,
            ModifiedUnixTime: DateTimeOffset.UtcNow.ToUnixTimeSeconds()
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

