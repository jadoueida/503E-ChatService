using ChatService.DTOs;
using ChatService.Storage;

namespace ChatService.Services;

public class ProfileService : IProfileService
{
    private readonly IProfileStore _profileStore;
    
    public ProfileService(IProfileStore profileStore)
    {
        _profileStore = profileStore;
    }
    

    public Task<Profile?> GetProfile(string username)
    {
        return _profileStore.GetProfile(username);
    }

    public Task UpdateProfile(Profile profile)
    {
        return _profileStore.UpsertProfile(profile);
    }

}