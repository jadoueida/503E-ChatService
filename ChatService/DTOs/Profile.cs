using System.ComponentModel.DataAnnotations;
namespace ChatService.DTOs;

public record Profile(
    [Required] string Username, 
    [Required] string FirstName, 
    [Required] string LastName,
    string ProfilePictureId
    );
    
    