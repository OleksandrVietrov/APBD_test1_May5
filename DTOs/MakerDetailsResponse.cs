namespace MakersApi.DTOs;

public class MakerDetailsResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public List<MakerProductResponse> Products { get; set; } = new();
}
