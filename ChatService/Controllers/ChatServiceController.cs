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
    private readonly IUserStore _userStore;
    private readonly BlobServiceClient _blobServiceClient;
    
    public ChatServiceController(IUserStore userStore, BlobServiceClient blobServiceClient, IConfiguration configuration)
    {
        _userStore = userStore;
        var connectionString = configuration.GetConnectionString("BlobStorage");
        _blobServiceClient = new BlobServiceClient(connectionString);
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
    public async Task<ActionResult<UploadImageResponse>> UploadImage([FromForm] UploadImageRequest request)
    {
        if (request.File == null)
        {
            return BadRequest("File is required");
        }

        // Get a reference to the blob container
        var containerName = "images";
        var container = _blobServiceClient.GetBlobContainerClient(containerName);

        // Generate a unique image ID
        var imageId = Guid.NewGuid().ToString();

        // Upload the image to the blob container
        var blobName = $"{imageId}.jpg";
        var blobClient = container.GetBlobClient(blobName);
        await blobClient.UploadAsync(request.File.OpenReadStream(), true);

        // Return the image ID in the response
        var response = new UploadImageResponse(imageId)
        {
            ImageId = imageId
        };
        return Ok(response);
    }

    public record UploadImageRequest(IFormFile File);
    public record UploadImageResponse(string ImageId);
}