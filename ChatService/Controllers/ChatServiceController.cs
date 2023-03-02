using Microsoft.AspNetCore.Mvc;
using ChatService.DTOs;
using Microsoft.Extensions.Configuration;
using ChatService.Storage;
using Azure.Storage.Blobs;

namespace ChatService.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatServiceController : ControllerBase
{
    private readonly IImageStore _imageStore;
    private readonly IUserStore _userStore;
    
    
    public ChatServiceController(IUserStore userStore, IImageStore imageStore)
    {
        _userStore = userStore;
        _imageStore = imageStore;
        
        
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
    
    
    
    [HttpPost]
    [Route("image")]
    public async Task<ActionResult<ImageResponse>> UploadImage([FromForm] Image request)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest("File cannot be null or empty.");
        }

        await _imageStore.UploadImage(request.File);

        return Ok();
    }
    
    
    
    
    [HttpGet("{id}")]
    public async Task<ActionResult<FileContentResult>> DownloadImage(string id)
    {
        var image = await _imageStore.GetImageById(id);

        if (image == null)
        {
            return NotFound("This image does not exist");
        }

        return Ok(image.File);
    }
}
    
