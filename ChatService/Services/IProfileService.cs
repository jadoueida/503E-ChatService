using ChatService.DTOs;

namespace ChatService.Services;

public interface IProfileService
{
    Task UpdateProfile(Profile profile);
    
    Task<Profile?> GetProfile(string username);

}