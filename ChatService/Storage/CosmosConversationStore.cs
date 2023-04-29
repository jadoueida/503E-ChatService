using System.Net;
using Microsoft.Azure.Cosmos;
using ChatService.DTOs;
using ChatService.Storage.Entities;

namespace ChatService.Storage;

public class CosmosConversationStore : IConversationStore
{
    private readonly CosmosClient _cosmosClient;


    public CosmosConversationStore(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    private Container Container => _cosmosClient.GetDatabase("conversations").GetContainer("conversations");


    public async Task CreateConvo(Conversation conversation)
    {
        if (conversation == null ||
            string.IsNullOrWhiteSpace(conversation.ConversationId)||
            string.IsNullOrWhiteSpace(conversation.ModifiedUnixTime.ToString()))
        {
            throw new ArgumentException($"Invalid conversation {conversation}", nameof(conversation));
        }

        var conversationEntity = ToEntity(conversation);
        await Container.UpsertItemAsync(conversationEntity);
        return;
    }


    private static ConversationEntity ToEntity(Conversation conversation)
    {
        return new ConversationEntity(
            Id: conversation.ConversationId,
            ModifiedUnixTime: conversation.ModifiedUnixTime
        );
    }
    
}