using System.Net;
using Azure;
using Azure.Storage.Blobs;
using ChatService.DTOs;
using System.Text.Json;
using ChatService.Storage.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Azure.Cosmos;

namespace ChatService.Storage;

public class BlobImageStorage : IImageStore
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public BlobImageStorage(BlobServiceClient blobServiceClient, IConfiguration configuration)
    {
        _blobServiceClient = blobServiceClient;
        _containerName = configuration.GetValue<string>("BlobStorage:ContainerName");
    }
    
    public async Task<Image?> GetImageById(string id)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(id);

        try
        {
            var response = await blobClient.DownloadAsync();
            using (var ms = new MemoryStream())
            {
                await response.Value.Content.CopyToAsync(ms);
                var imageBytes = ms.ToArray();
                if (imageBytes == null)
                {
                    return null;
                }
                
                var image = new Image(new FormFile(new MemoryStream(imageBytes), 0, imageBytes.Length, id, id));
                //remember to add id above
                return image;
            }
        }
        catch (RequestFailedException ex)
        {
            if (ex.Status == (int)HttpStatusCode.NotFound)
            {
                return null;
            }
            else
            {
                throw;
            }
        }
    }
    
    public async Task<string> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File cannot be null or empty.");
        }
        var imageId = Guid.NewGuid().ToString();
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(imageId);
        using (var stream = file.OpenReadStream())
        {
            await blobClient.UploadAsync(stream);
        }

        return imageId;
    }
    
    public async Task DeleteImage(string imageId)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(imageId);
            var response = await blobClient.DeleteIfExistsAsync();
        }
        catch (RequestFailedException)
        {
            // Log the exception here
        }
    }
    
   //private static ImageEntity ToEntity(DTOs.Image image)
   // {
    //    return new ChatService.Storage.Entities.ImageEntity(
    //        partitionKey: image.ImageId,
     //       ImageId: image.ImageId,
     //       file: image.File
     //   );
   // }

   // private static Image ToImage(ChatService.Storage.Entities.ImageEntity entity, byte[] data)
   // {
    //    return new DTOs.Image(
    //       ImageId: entity.ImageId, 
     //      File: entity.file
     //   );
    //}
}