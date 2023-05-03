namespace ChatService.Storage.Entities;

public record ConversationEntity(
    string id,
    long modifiedUnixTime
);
