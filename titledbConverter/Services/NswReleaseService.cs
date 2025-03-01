using System.Xml.Linq;
using titledbConverter.Extensions;
using titledbConverter.Models;
using titledbConverter.Services.Interface;

namespace titledbConverter.Services;

public class NswReleaseService : INswReleaseService
{
    private readonly IDbService _dbService;

    public NswReleaseService(IDbService dbService)
    {
        _dbService = dbService;
    }
    /// <summary>
    /// Parse the XML file and import all releases into the database
    /// </summary>
    /// <param name="xmlFilePath">Path to the XML file containing release information</param>
    /// <returns>Number of records successfully imported</returns>
    public async Task<int> ImportReleasesFromXmlAsync(string xmlFilePath)
    {
        try
        {
            var doc = XDocument.Load(xmlFilePath);
            var releases = ParseReleases(doc);

            // Bulk insert the releases into the database
            //await _dbContext.BulkInsertAsync(releases);

            return releases.Count;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error importing releases: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Parse the XML document and convert it to a list of NswReleaseTitle objects
    /// </summary>
    private List<NswReleaseTitle> ParseReleases(XDocument document)
    {
        var releases = new List<NswReleaseTitle>();

        // Query the XML to get all release elements
        var releaseElements = document.Descendants("release");

        // Process each release element
        foreach (var releaseElement in releaseElements)
        {
                
            var (cleanTitle, revision) = TitleParser.ExtractTitleAndRevision(GetElementValue(releaseElement, "name"));
            var (applicationId, version) = TitleParser.ExtractApplicationIdAndVersion(GetElementValue(releaseElement, "titleid"));

            var release = new NswReleaseTitle
            {
                Id = int.Parse(GetElementValue(releaseElement, "id")),
                ApplicationId = applicationId,
                TitleName = cleanTitle,
                Revision = revision,
                Publisher = GetElementValue(releaseElement, "publisher"),
                Region = GetElementValue(releaseElement, "region"),
                Languages = GetElementValue(releaseElement, "languages"),
                Firmware = GetElementValue(releaseElement, "firmware"),
                Version = version
            };

            releases.Add(release);
        }

        return releases;
    }

    /// <summary>
    /// Helper method to safely get element value or return empty string if element doesn't exist
    /// </summary>
    private static string GetElementValue(XElement parent, string elementName)
    {
        var element = parent.Element(elementName);
        return element != null ? element.Value : string.Empty;
    }

    
}