using System.Net;
using ChatService.DTOs;
using ChatService.Storage.Entities;
using Microsoft.Azure.Cosmos;

namespace ChatService.Storage;

public class CosmosProfileStorage : IProfileStore
{
    private readonly CosmosClient _cosmosClient;
    
    public CosmosProfileStorage(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }
    
    private Container Container => _cosmosClient.GetDatabase("users").GetContainer("users");

    public async Task UpsertProfile(Profile profile)
    {
        if (profile == null ||
            string.IsNullOrWhiteSpace(profile.Username) ||
            string.IsNullOrWhiteSpace(profile.FirstName) ||
            string.IsNullOrWhiteSpace(profile.LastName) ||
            string.IsNullOrWhiteSpace(profile.ProfilePictureId)
           )
        {
            throw new ArgumentException($"Invalid profile {profile}", nameof(profile));
        }

        await Container.UpsertItemAsync(ToEntity(profile));
    }
    
    public async Task<Profile?> GetProfile(string username)
    {
        try
        {
            var entity = await Container.ReadItemAsync<ProfileEntity>(
                id: username,
                partitionKey: new PartitionKey(username),
                new ItemRequestOptions
                {
                    ConsistencyLevel = ConsistencyLevel.Session
                }
            );
            return ToProfile(entity);
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
    
    public async Task DeleteProfile(string username)
    {
        try
        {
            await Container.DeleteItemAsync<Profile>(
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
    private static ProfileEntity ToEntity(Profile profile)
    {
        return new ProfileEntity(
            partitionKey: profile.Username,
            id: profile.Username,
            profile.FirstName,
            profile.LastName,
            profile.ProfilePictureId
        );
    }

    private static Profile ToProfile(ProfileEntity entity)
    {
        return new Profile(
            Username: entity.id,
            entity.FirstName,
            entity.LastName,
            entity.ProfilePicId
        );
    }
}