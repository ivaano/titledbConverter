using titledbConverter.Commands;

namespace titledbConverter.Services.Interface;

public interface ITitleDbService
{
    //public Task ImportRegionAsync(string regionFile);
    
    public Task ImportAllRegionsAsync(ConvertToSql.Settings settings);
    
    public Task ImportCnmtsAsync(string cnmtsFile);
    
    public Task ImportVersionsAsync(string versionsFile);
}