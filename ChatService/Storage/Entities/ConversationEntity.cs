namespace ChatService.Storage.Entities;

public record ConversationEntity(
    string id,
    string Username1,
    string Username2,
    long ModifiedUnixTime
);
