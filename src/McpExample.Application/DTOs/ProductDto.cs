namespace McpExample.Application.DTOs;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string Category,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
