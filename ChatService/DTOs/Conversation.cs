namespace ChatService.DTOs;

public record Conversation(
    string ConversationId,
    long ModifiedUnixTime
    );