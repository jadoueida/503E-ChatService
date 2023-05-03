using System.Net;
using System.Runtime.CompilerServices;
using Microsoft.Azure.Cosmos;
using ChatService.DTOs;
using ChatService.Storage.Entities;

namespace ChatService.Storage;

public class CosmosMessageStore : IMessageStore
{
    private readonly CosmosClient _cosmosClient;


    public CosmosMessageStore(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    private Container Container => _cosmosClient.GetDatabase("messages").GetContainer("messages");


    public async Task<long> AddMessage(DTOs.Message message)
    {
        if (message == null ||
            string.IsNullOrWhiteSpace(message.MessageId) ||
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
    
    public async Task<List<Message>> GetConversationMessages(string conversationId, int offset, int limit, long lastSeenMessageTime)
    {
        var queryDefinition = new QueryDefinition("SELECT * FROM Messages WHERE Messages.ConversationId = @conversationId AND Messages.CreatedUnixTime >= @lastSeenMessageTime ORDER BY Messages.CreatedUnixTime OFFSET @offset LIMIT @limit")
            .WithParameter("@conversationId", conversationId)
            .WithParameter("@lastSeenMessageTime", lastSeenMessageTime)
            .WithParameter("@offset", offset)
            .WithParameter("@limit", limit);
        var queryIterator = Container.GetItemQueryIterator<Message>(queryDefinition);

        var messages = new List<Message>();
        while (queryIterator.HasMoreResults)
        {
            var response = await queryIterator.ReadNextAsync();
            messages.AddRange(response.ToList());
        }

        return messages;
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
            id: message.MessageId,
            ConversationId: message.ConversationId,
            SenderUsername: message.SenderUsername,
            Text: message.Text,
            CreatedUnixTime: message.CreatedUnixTime
        );
    }
    
    private static Message? ToMessage(MessageEntity entity)
    {
        return new Message(
            MessageId: entity.id,
            ConversationId: entity.ConversationId,
            SenderUsername: entity.SenderUsername,
            Text:entity.Text,
            CreatedUnixTime:entity.CreatedUnixTime
        );
    }
}