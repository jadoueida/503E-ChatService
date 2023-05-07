namespace ChatService.DTOs;

public record ConversationsResponse(
    List<Conversation> Conversations,
    string? NextUri
);
