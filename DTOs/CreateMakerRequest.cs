using System.ComponentModel.DataAnnotations;

namespace MakersApi.DTOs;

public class CreateMakerRequest
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    public List<CreateMakerProductRequest>? Products { get; set; }
}
