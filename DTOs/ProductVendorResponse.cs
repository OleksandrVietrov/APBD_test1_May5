namespace MakersApi.DTOs;

public class ProductVendorResponse
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int Amount { get; set; }
    public decimal PricePerUnit { get; set; }
}
