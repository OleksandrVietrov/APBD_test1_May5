using MakersApi.DTOs;
using MakersApi.Repositories;

namespace MakersApi.Services;

public class MakerService : IMakerService
{
    private readonly IMakerRepository _repository;

    public MakerService(IMakerRepository repository)
    {
        _repository = repository;
    }

    public Task<MakerDetailsResponse?> GetByIdAsync(int id)
        => _repository.GetByIdAsync(id);

    public Task CreateAsync(CreateMakerRequest request)
        => _repository.CreateAsync(request);
}
