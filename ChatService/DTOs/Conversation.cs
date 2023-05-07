namespace ChatService.DTOs;

public record Conversation(
    string ConversationId,
    string Username1,
    string Username2,
    long ModifiedUnixTime
);