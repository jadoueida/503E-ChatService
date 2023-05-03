namespace ChatService.Storage.Entities;

public record ConversationParticipantEntity(
    string id,
    string conversationId,
    string participantUserId
    );