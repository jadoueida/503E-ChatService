using System.Net;
using ChatService.Storage.Entities;
using Microsoft.Azure.Cosmos;
using User = ChatService.DTOs.User;

namespace ChatService.Storage;

public class CosmosUserStorage : IUserStore
{
    private readonly CosmosClient _cosmosClient;
    
    public CosmosUserStorage(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }
    
    private Container Container => _cosmosClient.GetDatabase("users").GetContainer("users");

    public async Task UpsertUser(User user)
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
    
    public async Task<User?> GetUser(string username)
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
            await Container.DeleteItemAsync<User>(
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
    
    
    //WHY STATIC
    private static UserEntity ToEntity(User user)
    {
        return new UserEntity(
            partitionKey: user.Username,
            id: user.Username,
            user.FirstName,
            user.LastName,
            user.ProfilePicId
        );
    }

    private static User ToUser(UserEntity entity)
    {
        return new User(
            Username: entity.id,
            entity.FirstName,
            entity.LastName,
            entity.ProfilePicId
        );
    }
}