using System.Net;
using ChatService.DTOs;
using ChatService.Storage.Entities;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using ServiceStack;

namespace ChatService.Storage;

public class CosmosMessageStore : IMessageStore
{
    private readonly CosmosClient _cosmosClient;


    public CosmosMessageStore(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    private Container Container => _cosmosClient.GetDatabase("messages").GetContainer("messages");


    public async Task<long> AddMessage(Message message)
    {
        if (message == null ||
            string.IsNullOrWhiteSpace(message.id) ||
            string.IsNullOrWhiteSpace(message.SenderUsername) ||
            string.IsNullOrWhiteSpace(message.Text) ||
            string.IsNullOrWhiteSpace(message.ConversationId) ||
            string.IsNullOrWhiteSpace(message.CreatedUnixTime.ToString()))
                
        {
            throw new ArgumentException($"Invalid message {message}", nameof(message));
        }

        var messageEntity = ToEntity(message);
        await Container.UpsertItemAsync(messageEntity);
        return messageEntity.CreatedUnixTime;
    }
    
    public async Task<(List<Message> Messages, string ContinuationToken)> GetConversationMessages(string conversationId, string? continuationToken, int limit, long lastSeenMessageTime)
    {
        var queryDefinition = new QueryDefinition(
                "SELECT * FROM Messages WHERE Messages.ConversationId = @conversationId AND Messages.CreatedUnixTime >= @lastSeenMessageTime ORDER BY Messages.CreatedUnixTime")
            .WithParameter("@conversationId", conversationId)
            .WithParameter("@lastSeenMessageTime", lastSeenMessageTime);

        var queryOptions = new QueryRequestOptions
        {
            MaxItemCount = limit
        };

        var messages = new List<Message>();
        string newContinuationToken = null;
        FeedIterator<MessageEntity>? queryResult = null;
        
        if (continuationToken == null)
        {
            queryResult = Container.GetItemQueryIterator<MessageEntity>(queryDefinition, requestOptions: queryOptions);
        }
        else{queryResult = Container.GetItemQueryIterator<MessageEntity>(queryDefinition, requestOptions: queryOptions, continuationToken:continuationToken);}

        while (queryResult.HasMoreResults && messages.Count < limit)
        {
            var response = await queryResult.ReadNextAsync();
            messages.AddRange(response.Select(r => new Message(r.id, r.ConversationId, r.SenderUsername, r.Text,r.CreatedUnixTime)).ToList());
            newContinuationToken = response.ContinuationToken;
        }

        return (messages, newContinuationToken);
    }
    

    public async Task<Message?> GetMessage(string messageId)
{
     try
     {
         var entity = await Container.ReadItemAsync<MessageEntity>(
            id: messageId,
            partitionKey: new PartitionKey(messageId),
            new ItemRequestOptions
            {
                ConsistencyLevel = ConsistencyLevel.Session
            }
         );
        return ToMessage(entity);
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


    private static MessageEntity ToEntity(Message message)
    {
        return new MessageEntity(
            id: message.id,
            ConversationId: message.ConversationId,
            SenderUsername: message.SenderUsername,
            Text: message.Text,
            CreatedUnixTime: message.CreatedUnixTime
        );
    }
    
    private static Message ToMessage(MessageEntity entity)
    {
        return new Message(
            id: entity.id,
            ConversationId: entity.ConversationId,
            SenderUsername: entity.SenderUsername,
            Text:entity.Text,
            CreatedUnixTime:entity.CreatedUnixTime
        );
    }
}