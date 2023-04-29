using ChatService.DTOs;
using ChatService.Storage;

namespace ChatService.Services;

public class ImageService : IImageService
{
    
    private readonly IImageStore _imageStore;
    
    public ImageService(IImageStore imageStore)
    {
        _imageStore = imageStore;
    }
    
    public Task<string> UploadImage(IFormFile file)
    {
        return _imageStore.UploadImage(file);
    }

    public Task<Image?> GetImageById(string imageId)
    {
        return _imageStore.GetImageById(imageId);
    }
}