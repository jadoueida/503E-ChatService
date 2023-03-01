using Microsoft.AspNetCore.Mvc;
using ChatService.DTOs;
using ChatService.Storage;

namespace ChatService.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatServiceController : ControllerBase
{
    private readonly IUserStore _userStore;
    
    public ChatServiceController(IUserStore userStore)
    {
        _userStore = userStore;
    }
    
    
    [HttpPost]
    public async Task<ActionResult<User>> AddUser(User user)
    {
        var existingProfile = await _userStore.GetUser(user.Username);
        if (existingProfile != null)
        {
            return Conflict($"A user with username {user.Username} already exists");
        }

        await _userStore.UpsertUser(user);
        return CreatedAtAction(nameof(GetUser), new {username = user.Username},
            user); 
    }
    
    [HttpGet("{username}")]
    public async Task<ActionResult<User>> GetUser(string username)
    {
        var user = await _userStore.GetUser(username);
        if (user == null)
        {
            return NotFound($"A User with username {username} was not found");
        }
            
        return Ok(user);
    }
}