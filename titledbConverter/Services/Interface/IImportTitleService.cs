﻿namespace titledbConverter.Services.Interface;

public interface IImportTitleService
{
    public Task ImportTitlesFromFileAsync(string file);
    public Task ImportAllCategories();
    public Task ImportRatingContents(string file);
}