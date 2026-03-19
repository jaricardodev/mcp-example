using System.ComponentModel.DataAnnotations;

namespace McpExample.Application.DTOs;

public record UpdateProductDto(
    [Required][MaxLength(200)] string Name,
    [MaxLength(1000)] string Description,
    [Range(0, double.MaxValue)] decimal Price,
    [Range(0, int.MaxValue)] int StockQuantity,
    [MaxLength(100)] string Category
);
