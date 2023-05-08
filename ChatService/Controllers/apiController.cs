 using Microsoft.AspNetCore.Mvc;
using ChatService.DTOs;
using Microsoft.Extensions.Configuration;
using ChatService.Storage;
using Azure.Storage.Blobs;
using ChatService.Services;

namespace ChatService.Controllers;

[ApiController]
[Route("[controller]")]
public class apiController : ControllerBase
{
    private readonly IImageService _imageService;
    private readonly IProfileService _profileService;
    private readonly IMessageService _messageService;
    private readonly IConversationService _conversationService;
    private readonly ILogger<apiController> _logger;


    public apiController(IProfileService profileService, IImageService imageService, IMessageService messageService,
        IConversationService conversationService, ILogger<apiController> logger)
    {
        _profileService = profileService;
        _imageService = imageService;
        _messageService = messageService;
        _conversationService = conversationService;
        _logger = logger;
    }
    
    
    [HttpPost]
    [Route("profile")]
    public async Task<ActionResult<Profile>> AddProfile(Profile profile)
    {
        using(_logger.BeginScope("{Username}",profile.Username))
        {
            var existingProfile = await _profileService.GetProfile(profile.Username);
            if (existingProfile != null)
            {
                return Conflict($"A user with username {profile.Username} already exists");
            }

            await _profileService.UpdateProfile(profile);
            return CreatedAtAction(nameof(GetProfile), new { username = profile.Username },
                profile);
        }
    }
    
    [HttpGet("profile/{username}")]
    public async Task<ActionResult<Profile>> GetProfile(string username)
    {
        using(_logger.BeginScope("{Username}",username))
        {
            var profile = await _profileService.GetProfile(username);
            if (profile == null)
            {
                return NotFound($"A User with username {username} was not found");
            }

            return Ok(profile);
        }
    }
    
    
    
    [HttpPost]
    [Route("images")]
    public async Task<ActionResult<ImageResponse>> UploadImage([FromForm] Image request)
    {
        if (request.File == null || request.File.Length == 0)
        { 
            return BadRequest("File cannot be null or empty.");
        }
        var imageId = await _imageService.UploadImage(request.File);
        return new ImageResponse(imageId);
    }
    
    
    
    
    [HttpGet("images/{id}")]
    public async Task<ActionResult<byte[]>> DownloadImage(string id)
    {
        using(_logger.BeginScope("{ImageId}",id))
        {
            var image = await _imageService.GetImageById(id);

            if (image == null)
            {
                return NotFound("This image does not exist");
            }

            return Ok(image.File);
        }
    }


    [HttpPost]
    [Route("conversations")]
    public async Task<ActionResult<Conversation>> StartConversation(ConversationRequest request)
    {
        using(_logger.BeginScope("{Username1},{Username2}",request.Participants[0],request.Participants[1]))
        {
            if ((request.Participants == null) ||
                (request.Participants.Count != 2))
            {
                return BadRequest("Two Participants are Required");
            }

            var existingProfile1 = await _profileService.GetProfile(request.Participants[0]);
            var existingProfile2 = await _profileService.GetProfile(request.Participants[1]);
            if ((existingProfile1 == null) || (existingProfile2 == null))
            {
                return NotFound("Either one or both of the mentioned participants don't exist");
            }

            string potentialConversationId1 = request.Participants[0] + "_" + request.Participants[1];
            string potentialConversationId2 = request.Participants[1] + "_" + request.Participants[0];
            var existingConversation1 = await _conversationService.GetConversationById(potentialConversationId1);
            var existingConversation2 = await _conversationService.GetConversationById(potentialConversationId2);
            if ((existingConversation1 != null) ||
                (existingConversation2 != null))
            {
                return Conflict("The conversation already exists");
            }



            // 409 if conversation exists

            Conversation response = await _conversationService.CreateConvo(request);


            return CreatedAtAction(nameof(StartConversation), response);
        }
    }

    [HttpPost]
    [Route("conversations/{conversationId}/messages")]
    public async Task<ActionResult<MessageResponse>> AddMessage(MessageRequest message, string conversationId)
    {
        using(_logger.BeginScope("{MessageId},{ConversationId}",message.MessageId,conversationId))
        {
            var existingMessage = await _messageService.GetMessage(message.MessageId);
            if (existingMessage != null)
            {
                return Conflict($"A message with MessageId {message.MessageId} already exists");
            }

            var existingConversation = await _conversationService.GetConversationById(conversationId);
            if (existingConversation == null)
            {
                return NotFound("The requested conversation does not exist");
            }

            if ((message.SenderUsername != existingConversation.Username1) &&
                (message.SenderUsername != existingConversation.Username2))
            {
                return BadRequest("The Sender User is not a part of the mentioned conversation exist");
            }


            long createdUnixTime = await _messageService.AddMessage(message, conversationId);
            MessageResponse messageResponse = new MessageResponse(createdUnixTime);
            return CreatedAtAction(nameof(AddMessage), messageResponse);
        }
    }

    [HttpGet]
    [Route("conversations/{conversationId}/messages")]
    public async Task<ActionResult<MessagesResponse>> GetConvoMessages(string conversationId, string? continuationToken = null, int limit = 50,long lastSeenMessageTime = 0)
    {
        using(_logger.BeginScope("{ConversationId}",conversationId))
        {
            var existingConversation = await _conversationService.GetConversationById(conversationId);
            if (existingConversation == null)
            {
                return NotFound("The requested conversation does not exist");
            }

            var messages =
                await _messageService.GetConversationMessages(conversationId, continuationToken, limit,
                    lastSeenMessageTime);
            return Ok(messages);
        }
    }
    
    [HttpGet]
    [Route("conversations")]
    public async Task<ActionResult<ConversationsResponse>> GetConversations(string username, string? continuationToken = null, int limit=50, long lastSeenConversationTime=0)
    {
        using(_logger.BeginScope("{Username}",username))
        {
            var existingUser = await _profileService.GetProfile(username);
            if (existingUser == null)
            {
                return NotFound("The user of the requested conversations does not exist");
            }

            var conversations =
                await _conversationService.GetConversations(username, continuationToken, limit,
                    lastSeenConversationTime);
            return Ok(conversations);
        }
        
    }

}


    
