namespace Nop.Plugin.Misc.InfigoMarketplace.Models.Import;

public record DownloadedFile(byte[] Bytes, string FileName, string Extension, string ContentType);
