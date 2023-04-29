namespace ChatService.DTOs;

public record Message(
    string MessageId,
    string ConversationId,
    string SenderUsername,
    string Text,
    long CreatedUnixTime
);

    
