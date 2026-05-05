using MakersApi.DTOs;

namespace MakersApi.Services;

public interface IMakerService
{
    Task<MakerDetailsResponse?> GetByIdAsync(int id);
    Task CreateAsync(CreateMakerRequest request);
}
