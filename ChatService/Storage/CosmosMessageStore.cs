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
}