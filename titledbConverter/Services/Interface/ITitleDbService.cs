using titledbConverter.Commands;

namespace titledbConverter.Services.Interface;

public interface ITitleDbService
{
    //public Task ImportRegionAsync(string regionFile);
    
    public Task MergeAllRegionsAsync(ConvertToSql.Settings settings);

    public Task<Dictionary<string, List<string>>?> GetRegionLanguages(string fileLocation);

}