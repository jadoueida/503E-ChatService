namespace ChatService.Storage.Entities;

public record ConversationEntity(
    string Id,
    long ModifiedUnixTime
);
