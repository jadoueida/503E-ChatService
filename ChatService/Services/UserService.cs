using ChatService.DTOs;
using ChatService.Storage;

namespace ChatService.Services;

public class UserService : IUserService
{
    private readonly IUserStore _userStore;
    
    public UserService(IUserStore userStore)
    {
        _userStore = userStore;
    }
    

    public Task<User?> GetUser(string username)
    {
        return _userStore.GetUser(username);
    }

    public Task UpdateUser(User user)
    {
        return _userStore.UpsertUser(user);
    }

}