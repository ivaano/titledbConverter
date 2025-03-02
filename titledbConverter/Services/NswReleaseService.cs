using System.Text.RegularExpressions;
using System.Xml.Linq;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using titledbConverter.Data;
using titledbConverter.Extensions;
using titledbConverter.Models;
using titledbConverter.Services.Interface;
using titledbConverter.Utils;

namespace titledbConverter.Services;

public class NswReleaseService(SqliteDbContext dbContext) : INswReleaseService
{
 
    
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
            
            await dbContext.Database.ExecuteSqlAsync($"DELETE FROM NswReleaseTitles");
            await dbContext.Database.ExecuteSqlAsync($"DELETE FROM sqlite_sequence WHERE name = 'NswReleaseTitles'");
            await dbContext.BulkInsertAsync(releases);

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

        var releaseElements = document.Descendants("release");

        foreach (var releaseElement in releaseElements)
        {
            var id = GetElementValue(releaseElement, "id");
            var titleName = GetElementValue(releaseElement, "name");
            var titleId = GetElementValue(releaseElement, "titleid");
            var titles = TitleParser.ExtractTitleIds(titleId);

            if (titles.Count > 1)
            {
                
                foreach (var additionalTitle in titles)
                {
                    var cleanTitleName = Regex.Replace(titleName, @"\[.*?\]", "").Trim();
                    var releaseSameTitle = new NswReleaseTitle
                    {
                        Id = int.Parse(GetElementValue(releaseElement, "id")),
                        ApplicationId = additionalTitle.ApplicationId,
                        TitleName = cleanTitleName,
                        Publisher = GetElementValue(releaseElement, "publisher"),
                        Region = GetElementValue(releaseElement, "region"),
                        Languages = GetElementValue(releaseElement, "languages"),
                        Firmware = GetElementValue(releaseElement, "firmware"),
                        Version = additionalTitle.Version
                    };
                    releases.Add(releaseSameTitle);
                }
                continue;
            }

            var (applicationId, version) = TitleParser.ExtractApplicationIdAndVersion(
                titleId);
            var (cleanTitle, revision) = TitleParser.ExtractTitleAndRevision(
                titleName);
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
            //only save valid titleId, I've seen titleIds with incomplete numbers (Last Fight)
            if (release.ApplicationId.Length == 16) releases.Add(release);
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