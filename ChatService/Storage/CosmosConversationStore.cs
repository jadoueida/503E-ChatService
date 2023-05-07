using System.Net;
using Microsoft.Azure.Cosmos;
using ChatService.DTOs;
using ChatService.Storage.Entities;
using Newtonsoft.Json;

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
            string.IsNullOrWhiteSpace(conversation.Username1)||
            string.IsNullOrWhiteSpace(conversation.Username2)||
            string.IsNullOrWhiteSpace(conversation.ModifiedUnixTime.ToString()))
        {
            throw new ArgumentException($"Invalid conversation {conversation}", nameof(conversation));
        }

        var conversationEntity = ToEntity(conversation);
        await Container.UpsertItemAsync(conversationEntity);
        return;
    }
    
    public async Task<(List<Conversation> Conversations, string ContinuationToken)> GetConversations(string username, string? continuationToken, int limit, long lastSeenConversationTime)
    {
        var queryDefinition = new QueryDefinition(
                "SELECT TOP @limit * FROM conversations WHERE conversations.ModifiedUnixTime > @lastSeenConversationTime AND (conversations.Username1 = @username OR conversations.Username2 = @username) ORDER BY conversations.ModifiedUnixTime")
            .WithParameter("@username", username)
            .WithParameter("@lastSeenConversationTime", lastSeenConversationTime);

        var queryOptions = new QueryRequestOptions
        {
            MaxItemCount = limit
        };

        var conversations = new List<Conversation>();
        string newContinuationToken = null;

        var queryResult = Container.GetItemQueryIterator<ConversationEntity>(queryDefinition, requestOptions: queryOptions, continuationToken:continuationToken);

        while (queryResult.HasMoreResults && conversations.Count < limit)
        {
            var response = await queryResult.ReadNextAsync();
            conversations.AddRange(response.Select(r => new Conversation(r.id, r.Username1, r.Username2, r.ModifiedUnixTime)).ToList());
            newContinuationToken = JsonConvert.SerializeObject(response.ContinuationToken);
        }

        return (conversations, newContinuationToken);
    }

    public async Task<(List<Conversation> Conversations, string ContinuationToken)> GetFirstConversations(string username, int limit, long lastSeenConversationTime)
    {
        var queryDefinition = new QueryDefinition(
                "SELECT * FROM conversations WHERE conversations.ModifiedUnixTime > @lastSeenConversationTime AND (conversations.Username1 = @username OR conversations.Username2 = @username) ORDER BY conversations.ModifiedUnixTime")
            .WithParameter("@username", username)
            .WithParameter("@lastSeenConversationTime", lastSeenConversationTime);

        var queryOptions = new QueryRequestOptions
        {
            MaxItemCount = limit,
        };

        var conversations = new List<Conversation>();
        string newContinuationToken = null;

        var queryResult = Container.GetItemQueryIterator<ConversationEntity>(queryDefinition, requestOptions: queryOptions);

        while (queryResult.HasMoreResults && conversations.Count < limit)
        {
            var response = await queryResult.ReadNextAsync();
            conversations.AddRange(response.Select(r => new Conversation(r.id, r.Username1, r.Username2, r.ModifiedUnixTime)));
            newContinuationToken = JsonConvert.SerializeObject(response.ContinuationToken);
        }

        return (conversations, newContinuationToken);
    }


    public async Task UpdateConversationTime(string conversationId, long dateTime)
    {
        ConversationEntity entity = await Container.ReadItemAsync<ConversationEntity>(
            id: conversationId, 
            partitionKey: new PartitionKey(conversationId), 
            new ItemRequestOptions 
            { 
                ConsistencyLevel = ConsistencyLevel.Session
            }
        );
        var conversation = entity with { ModifiedUnixTime = dateTime };
        
        await Container.ReplaceItemAsync(conversation, conversation.id);

    }

    public async Task<Conversation?> GetConversationById(string conversationId)
    {
        try
        {
            var entity = await Container.ReadItemAsync<ConversationEntity>(
                id: conversationId,
                partitionKey: new PartitionKey(conversationId),
                new ItemRequestOptions
                {
                    ConsistencyLevel = ConsistencyLevel.Session
                }
            );
            return ToConversation(entity);
        }
        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            throw;
        }
    }


    private static ConversationEntity ToEntity(Conversation conversation)
    {
        return new ConversationEntity(
            id: conversation.ConversationId,
            Username1: conversation.Username1,
            Username2: conversation.Username2,
            ModifiedUnixTime: conversation.ModifiedUnixTime
        );
    }
    
    private static Conversation? ToConversation(ConversationEntity entity)
    {
        return new Conversation(
            ConversationId: entity.id,
            Username1: entity.Username1,
            Username2: entity.Username2,
            ModifiedUnixTime: entity.ModifiedUnixTime
        );
    }
    
}