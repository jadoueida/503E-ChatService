using System.ComponentModel.DataAnnotations;
namespace ChatService.DTOs;

public record User(
    [Required] string Username, 
    [Required] string FirstName, 
    [Required] string LastName,
    [Required] string ProfilePicId);