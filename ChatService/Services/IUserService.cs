using ChatService.DTOs;

namespace ChatService.Services;

public interface IUserService
{
    Task UpdateUser(User user);
    
    Task<User?> GetUser(string username);

}