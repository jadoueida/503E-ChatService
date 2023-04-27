namespace ChatService.DTOs;

public record ConversationResponse(
    string ConversationId,
    int CreatedUnixTime
    );