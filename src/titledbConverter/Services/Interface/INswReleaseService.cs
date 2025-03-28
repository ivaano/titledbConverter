namespace titledbConverter.Services.Interface;

public interface INswReleaseService
{
    /// <summary>
    /// Parse the XML file and import all releases into the database
    /// </summary>
    /// <param name="xmlFilePath">Path to the XML file containing release information</param>
    /// <returns>Number of records successfully imported</returns>
    public Task<int> ImportReleasesFromXmlAsync(string xmlFilePath, bool overwrite = false);
    
    public Task<int> ImportReleasesFromDirectoryAsync(string directoryPath);

}