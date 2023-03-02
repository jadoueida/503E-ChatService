using ChatService.Storage;
namespace ChatService.Storage;
using ChatService.DTOs;

public interface IImageStore
{
    Task UploadImage(IFormFile file);
    Task<Image?> GetImageById(string imageId);
    Task DeleteImage(string imageId);
}