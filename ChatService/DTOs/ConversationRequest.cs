namespace ChatService.DTOs;

public record ConversationRequest(
    List<string> Participants,
    MessageRequest Message
    );