namespace Rise.Shared.Profile;

public interface IProfileService
{
    Task<Result<ProfileResponse.Envelope>> GetAsync(CancellationToken cancellationToken = default);
    Task<Result<ProfileResponse.Envelope>> UpdateAsync(ProfileRequest.UpdateProfile request, CancellationToken cancellationToken = default);
}
