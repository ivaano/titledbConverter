namespace titledbConverter.Services.Interface;

public interface ITitleDbService
{
    public Task ImportRegionAsync(string regionFile);
}