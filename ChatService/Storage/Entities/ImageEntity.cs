namespace ChatService.Storage.Entities;

public record ImageEntity(string partitionKey, string ImageId, IFormFile file);