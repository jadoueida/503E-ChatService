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
    
    public async Task<List<Conversation>> GetConversations(string username, int offset, int limit, long lastSeenConversationTime)
    {
        var queryDefinition = new QueryDefinition(
                "SELECT TOP(@limit) c.* FROM conversations c JOIN conversations-participants cp ON c.conversationId = cp.conversationId WHERE c.type = 'conversation' AND c.modifiedUnixTime > @lastSeenConversationTime AND cp.participantUsername = @username ORDER BY c.modifiedUnixTime OFFSET @offset;")
            .WithParameter("@username", username)
            .WithParameter("@lastSeenConversationTime", lastSeenConversationTime)
            .WithParameter("@offset", offset)
            .WithParameter("@limit", limit);

        var conversations = new List<Conversation>();

        var queryResult = Container.GetItemQueryIterator<Conversation>(queryDefinition);

        while (queryResult.HasMoreResults)
        {
            var response = await queryResult.ReadNextAsync();
            conversations.AddRange(response.Select(r => new Conversation(r.ConversationId, r.ModifiedUnixTime)).ToList());
        }

        return conversations;
    }

    public async Task UpdateConversationTime(string conversationId, long dateTime)
    {
        await Container.ReplaceItemAsync(new ConversationEntity(id:conversationId,modifiedUnixTime:dateTime), conversationId);
    }


    private static ConversationEntity ToEntity(Conversation conversation)
    {
        return new ConversationEntity(
            id: conversation.ConversationId,
            modifiedUnixTime: conversation.ModifiedUnixTime
        );
    }
    
}