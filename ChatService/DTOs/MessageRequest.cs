using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace ChatService.DTOs;

public record MessageRequest(
    string MessageId,
    string SenderUsername,
    string Text
);