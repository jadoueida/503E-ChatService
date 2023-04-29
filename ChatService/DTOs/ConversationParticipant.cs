namespace ChatService.DTOs;

public record ConversationParticipant(
    string conversationId,
    string participantUsername
    );