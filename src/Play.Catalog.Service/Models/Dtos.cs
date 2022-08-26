using System.ComponentModel.DataAnnotations;

namespace Play.Catalog.Service.Models;

public record ItemDto(Guid Id, string Name, string Description, decimal Price, DateTimeOffset CreatedDate);

public record CreateItemDto([Required] string Name, string Description, [Range(0, 1000)] decimal Price);

public record UpdateItemDto(Guid Id, [Required] string Name, string Description, [Range(0, 1000)] decimal Price);
