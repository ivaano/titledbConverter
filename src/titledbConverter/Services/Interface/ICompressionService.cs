namespace titledbConverter.Services.Interface;

public interface ICompressionService
{
    Task CompressFileAsync(string sourceFilePath, string targetFilePath);
}