using ChatService.DTOs;
using ChatService.Storage.Entities;
using Microsoft.Azure.Cosmos;
using ServiceStack;

namespace ChatService.Storage;

public class CosmosConversationParticipantStore : IConversationParticipantStore
{
    private readonly CosmosClient _cosmosClient;


    public CosmosConversationParticipantStore(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    private Container Container => _cosmosClient.GetDatabase("conversations-participants").GetContainer("conversations-participants");


    public async Task AddParticipant(ConversationParticipant conversationParticipant)
    {
        if (conversationParticipant == null ||
            string.IsNullOrWhiteSpace(conversationParticipant.conversationId)||
            string.IsNullOrWhiteSpace(conversationParticipant.participantUsername))
        {
            throw new ArgumentException($"Invalid conversationParticipant {conversationParticipant}", nameof(conversationParticipant));
        }

        var conversationParticipantEntity = ToEntity(conversationParticipant);
        await Container.UpsertItemAsync(conversationParticipantEntity);
        return ;
    }
    
    // public async Task<List<ConversationParticipant>> GetParticipantsByConversationId(string conversationId)
    // {
    //     var query = new QueryDefinition("SELECT * FROM c WHERE c.ConversationId = @conversationId")
    //         .WithParameter("@conversationId", conversationId);
    //
    //     List<ConversationParticipant> participants = new List<ConversationParticipant>();
    //     var queryIterator = Container.GetItemQueryIterator<ConversationParticipant>(query);
    //     {
    //         while (queryIterator.HasMoreResults)
    //         {
    //             var response = await queryIterator.ReadNextAsync();
    //             participants.AddRange(response.participantUsername);
    //         }
    //     }
    //     return participants;
    // }



    private static ConversationParticipantEntity ToEntity(ConversationParticipant conversationParticipant)
    {
        return new ConversationParticipantEntity(
            id: conversationParticipant.conversationId+conversationParticipant.participantUsername,
            conversationId: conversationParticipant.conversationId,
            participantUserId: conversationParticipant.participantUsername
        );
    }
}