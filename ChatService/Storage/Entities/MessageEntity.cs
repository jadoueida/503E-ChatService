namespace ChatService.Storage.Entities;

public record MessageEntity(
    string id,
    string ConversationId,
    string SenderUsername,
    string Text,
    long CreatedUnixTime
    );