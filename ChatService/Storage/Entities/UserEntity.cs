namespace ChatService.Storage.Entities;

public record UserEntity(string partitionKey, string id, string FirstName, string LastName, string ProfilePicId);