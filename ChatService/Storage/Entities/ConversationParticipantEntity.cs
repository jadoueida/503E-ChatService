namespace ChatService.Storage.Entities;

public record ConversationParticipantEntity(
    string conversationId,
    string participantUserId
    );