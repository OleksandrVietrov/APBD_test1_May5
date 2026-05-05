using System.ComponentModel.DataAnnotations;

namespace MakersApi.DTOs;

public class CreateMakerProductRequest
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    public decimal StrickerPrice { get; set; }

    [Required]
    [StringLength(50)]
    public string Type { get; set; } = null!;
}
