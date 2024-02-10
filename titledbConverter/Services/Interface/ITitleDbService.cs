﻿namespace titledbConverter.Services.Interface;

public interface ITitleDbService
{
    public Task ImportRegionAsync(string regionFile);
    
    public Task ImportCnmtsAsync(string cnmtsFile);
    
    public Task ImportVersionsAsync(string versionsFile);
}