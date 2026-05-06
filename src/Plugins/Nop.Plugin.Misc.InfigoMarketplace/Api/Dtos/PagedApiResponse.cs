namespace Nop.Plugin.Misc.InfigoMarketplace.Api.Dtos;

public record PagedApiResponse<T>(IReadOnlyList<T> Items, int TotalCount);
