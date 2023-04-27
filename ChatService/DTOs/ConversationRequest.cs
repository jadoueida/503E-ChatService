namespace ChatService.DTOs;

public record ConversationRequest(
    Array Participants,
    MessageRequest Message
    );