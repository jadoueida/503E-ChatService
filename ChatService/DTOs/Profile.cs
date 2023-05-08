using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace ChatService.DTOs;

public record Profile(
    [Required] string Username, 
    [Required] string FirstName, 
    [Required] string LastName,
    string? ProfilePictureId
    );
    
    