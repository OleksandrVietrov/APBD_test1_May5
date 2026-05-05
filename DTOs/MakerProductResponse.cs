namespace MakersApi.DTOs;

public class MakerProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal StrickerPrice { get; set; }
    public ProductTypeResponse ProductType { get; set; } = null!;
    public List<ProductVendorResponse> Vendors { get; set; } = new();

    
}
