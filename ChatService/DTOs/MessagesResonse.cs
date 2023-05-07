namespace ChatService.DTOs;

public record MessagesResponse(
    List<Message> Messages,
    string NextUri
    );