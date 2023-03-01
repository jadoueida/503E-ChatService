using ChatService.DTOs;
namespace ChatService.Storage;

public interface IUserStore
{
    Task UpsertUser(User user);
    Task<User?> GetUser(string username);
    Task DeleteUser(string username);
}