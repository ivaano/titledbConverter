using System.IO.Compression;
using titledbConverter.Services.Interface;

namespace titledbConverter.Services;

public class CompressionService : ICompressionService
{
    public Task CompressFileAsync(string sourceFilePath, string targetFilePath)
    {
        using var inputFileStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read);
        using var compressedFileStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write);
        using var gzipStream = new GZipStream(compressedFileStream, CompressionLevel.Optimal);
        inputFileStream.CopyTo(gzipStream);
        return Task.CompletedTask;
    }
}