using System.Net;
using Microsoft.Azure.Cosmos;
using ChatService.DTOs;
using ChatService.Storage.Entities;

namespace ChatService.Storage;

public class CosmosUserStorage : IUserStore
{
    private readonly CosmosClient _cosmosClient;
    
    public CosmosUserStorage(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }
    
    private Container Container => _cosmosClient.GetDatabase("users").GetContainer("users");

    public async Task UpsertUser(ChatService.DTOs.User user)
    {
        if (user == null ||
            string.IsNullOrWhiteSpace(user.Username) ||
            string.IsNullOrWhiteSpace(user.FirstName) ||
            string.IsNullOrWhiteSpace(user.LastName)
           )
        {
            throw new ArgumentException($"Invalid profile {user}", nameof(user));
        }

        await Container.UpsertItemAsync(ToEntity(user));
    }
    
    public async Task<DTOs.User?> GetUser(string username)
    {
        try
        {
            var entity = await Container.ReadItemAsync<UserEntity>(
                id: username,
                partitionKey: new PartitionKey(username),
                new ItemRequestOptions
                {
                    ConsistencyLevel = ConsistencyLevel.Session
                }
            );
            return ToUser(entity);
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
    
    public async Task DeleteUser(string username)
    {
        try
        {
            await Container.DeleteItemAsync<DTOs.User>(
                id: username,
                partitionKey: new PartitionKey(username)
            );
        }
        catch (CosmosException e)
        {
            if (e.StatusCode == HttpStatusCode.NotFound)
            {
                return;
            }

            throw;
        }
    }
    
    private static UserEntity ToEntity(DTOs.User user)
    {
        return new ChatService.Storage.Entities.UserEntity(
            partitionKey: user.Username,
            id: user.Username,
            user.FirstName,
            user.LastName,
            user.ProfilePicId
        );
    }

    private static DTOs.User ToUser(ChatService.Storage.Entities.UserEntity entity)
    {
        return new DTOs.User(
            Username: entity.id,
            entity.FirstName,
            entity.LastName,
            entity.ProfilePicId
        );
    }
}