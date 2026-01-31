using System.ComponentModel.DataAnnotations;

namespace MakeupStudio_Api.Dtos;

public class CreateServiceDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 600)]
    public int DurationMinutes { get; set; }

    [Range(0, 100000)]
    public decimal Price { get; set; }
}
