using Microsoft.AspNetCore.Mvc;
using ChatService.DTOs;
using Microsoft.Extensions.Configuration;
using ChatService.Storage;
using Azure.Storage.Blobs;
using ChatService.Services;

namespace ChatService.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatServiceController : ControllerBase
{
    private readonly IImageService _imageService;
    private readonly IUserService _userService;
    private readonly IMessageService _messageService;
    private readonly IConversationService _conversationService;


    public ChatServiceController(IUserService userService, IImageService imageService, IMessageService messageService, IConversationService conversationService)
    {
        _userService = userService;
        _imageService = imageService;
        _messageService = messageService;
        _conversationService = conversationService;
    }
    
    
    [HttpPost]
    public async Task<ActionResult<User>> AddUser(User user)
    {
        var existingProfile = await _userService.GetUser(user.Username);
        if (existingProfile != null)
        {
            return Conflict($"A user with username {user.Username} already exists");
        }
        await _userService.UpdateUser(user);
        return CreatedAtAction(nameof(GetUser), new {username = user.Username},
            user); 
    }
    
    [HttpGet("{username}")]
    public async Task<ActionResult<User>> GetUser(string username)
    {
        var user = await _userService.GetUser(username);
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
        var imageId =  await _imageService.UploadImage(request.File);
       return new ImageResponse(imageId);
    }
    
    
    
    
    [HttpGet("image/{id}")]
    
    public async Task<ActionResult<byte[]>> DownloadImage(string id)
    {
        var image = await _imageService.GetImageById(id);

        if (image == null)
        {
            return NotFound("This image does not exist");
        }

        return Ok(image.File);
    }


    [HttpPost]
    [Route("conversations")]
    public async Task<ActionResult<Conversation>> StartConversation(ConversationRequest request)
    {
        
        if ((request.Participants == null)||
            (request.Participants.Count != 2))
        {
            return BadRequest("Two Participants are Required");
        }
        
        var existingProfile1 = await _userService.GetUser(request.Participants[0]);
        var existingProfile2 = await _userService.GetUser(request.Participants[1]);
        if ((existingProfile1 == null) || (existingProfile2 == null))
        {
            return NotFound("Either one or both of the mentioned participants don't exist");
        }
        
        
        
        
        // 409 if conversation exists

        Conversation response = await _conversationService.CreateConvo(request);
        
    
        return CreatedAtAction(nameof(StartConversation), response);
    }

    [HttpPost]
    [Route("conversations/{conversationId}/messages")]
    public async Task<ActionResult<MessageResponse>> AddMessage(MessageRequest message, string conversationId)
    {
        var existingMessage = await _messageService.GetMessage(message.MessageId);
        if (existingMessage != null)
        {
            return Conflict($"A message with MessageId {message.MessageId} already exists");
        }
        

        long createdUnixTime =await _messageService.AddMessage(message,conversationId);
        MessageResponse messageResponse = new MessageResponse(createdUnixTime);
        return CreatedAtAction(nameof(AddMessage), messageResponse);

    }

    [HttpGet]
    [Route("conversations/{conversationId}/messages")]
    public async Task<ActionResult<List<Message>>> GetConvoMessages(string conversationId, int offset, int limit,long lastSeenMessageTime)
    {
        List<Message> messages = await _messageService.GetConversationMessages(conversationId, offset, limit,lastSeenMessageTime);
        return CreatedAtAction(nameof(GetConvoMessages), messages);
    }
    
    [HttpGet]
    [Route("conversations")]
    public async Task<IActionResult> GetConversations(string username, int offset, int limit, long lastSeenConversationTime)
    {
        username ??= "";
        offset = offset == 0 ? 0 : offset;
        limit = limit == 0 ? 50 : limit;
        lastSeenConversationTime = lastSeenConversationTime == 0 ? 0 : lastSeenConversationTime;


        var conversations = await _conversationService.GetConversations(username, offset, limit, lastSeenConversationTime);


        return Ok(conversations);
    }

}


    
