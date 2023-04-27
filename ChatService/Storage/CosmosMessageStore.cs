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


    public async Task<long> AddMessage(DTOs.MessageRequest message)
    {
        if (message == null ||
            string.IsNullOrWhiteSpace(message.MessageId) ||
            string.IsNullOrWhiteSpace(message.SenderUsername) ||
            string.IsNullOrWhiteSpace(message.Text))
        {
            throw new ArgumentException($"Invalid profile {message}", nameof(message));
        }

        var y = ToEntity(message);
        await Container.UpsertItemAsync(y);
        return y.CreatedUnixTime;
    }


//public async Task<DTOs.Message?> GetMessage(string MessageId)
//{
    // try
    // {
    //     var entity = await Container.ReadItemAsync<>(
    //        id: MessageId,
    //        partitionKey: new PartitionKey(MessageId),
    //       new ItemRequestOptions
    //     {
    //   ConsistencyLevel = ConsistencyLevel.Session
    //     }
    //  );
    //    return ToMessage(entity);
    //  }
    //  catch (CosmosException e)
    //  {
    //     if (e.StatusCode == HttpStatusCode.NotFound)
    //     {
    //         return null;
    //     }
    //     throw;
    //  }
//}


    private static MessageEntity ToEntity(DTOs.MessageRequest message)
    {
        return new ChatService.Storage.Entities.MessageEntity(
            id: message.MessageId,
            ConversationId: Guid.NewGuid().ToString(),
            SenderUsername: message.SenderUsername,
            Text: message.Text,
            CreatedUnixTime: DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        );
    }
}