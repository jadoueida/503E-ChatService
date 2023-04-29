using ChatService.DTOs;

namespace ChatService.Services;

public interface IImageService
{
    Task<string> UploadImage(IFormFile file);
    
    Task<Image?> GetImageById(string imageId);
    
}