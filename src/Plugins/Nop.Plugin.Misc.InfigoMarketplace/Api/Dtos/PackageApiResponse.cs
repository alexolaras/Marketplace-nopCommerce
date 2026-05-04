namespace Nop.Plugin.Misc.InfigoMarketplace.Api.Dtos;

public record PackageApiResponse(
    Guid Id,
    string Name,
    string Description,
    string Type,
    Guid? CategoryId,
    string? CategoryName,
    string CurrentVersion,
    List<string> Tags
);
