using ChatService.Storage.Entities;
using Microsoft.Azure.Cosmos;

namespace ChatService.Storage;

public class CosmosConversationParticipantStore : IConversationParticipantStore
{
    private readonly CosmosClient _cosmosClient;


    public CosmosConversationParticipantStore(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    private Container Container => _cosmosClient.GetDatabase("conversations").GetContainer("conversations-participants");


    public async Task AddParticipant(DTOs.ConversationParticipant conversationParticipant)
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


    private static ConversationParticipantEntity ToEntity(DTOs.ConversationParticipant conversationParticipant)
    {
        return new ConversationParticipantEntity(
            conversationId: conversationParticipant.conversationId,
            participantUserId: conversationParticipant.participantUsername
        );
    }
}