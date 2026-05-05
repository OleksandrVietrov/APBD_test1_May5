using MakersApi.DTOs;

namespace MakersApi.Repositories;

public interface IMakerRepository
{
    Task<MakerDetailsResponse?> GetByIdAsync(int id);
    Task CreateAsync(CreateMakerRequest request);
}
