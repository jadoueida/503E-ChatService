using System.ComponentModel.DataAnnotations;

namespace ChatService.DTOs;

public record Message(
    [Required] string id,
    [Required] string ConversationId,
    [Required] string SenderUsername,
    [Required] string Text,
    [Required] long CreatedUnixTime
);

    
