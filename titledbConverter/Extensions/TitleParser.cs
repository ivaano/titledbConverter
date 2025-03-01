using System.Text.RegularExpressions;

namespace titledbConverter.Extensions;

public static partial class TitleParser
{
    // Regex pattern to extract revision from title
    // Matches "[Rev X.X.X]" or similar patterns at the end of a string
    private static readonly Regex RevisionPattern = RevisionRegex();
    
    // Regex pattern to extract clean application ID and version
    // Matches "XXXXXXXXXXXXXXXX (vXXXXX)" where X are alphanumeric characters
    private static readonly Regex ApplicationIdPattern = VersionRegex();


    /// <summary>
    /// Extracts the base title and revision (if present) from a full title string
    /// </summary>
    /// <param name="fullTitle">The full title string potentially containing a revision</param>
    /// <returns>A tuple containing (cleanTitle, revision)</returns>
    public static (string CleanTitle, string? Revision) ExtractTitleAndRevision(string fullTitle)
    {
        if (string.IsNullOrEmpty(fullTitle))
            return (string.Empty, null);

        var match = RevisionPattern.Match(fullTitle);

        if (!match.Success) return (fullTitle.Trim(), null);
        var revision = match.Groups[1].Value;
        var cleanTitle = RevisionPattern.Replace(fullTitle, string.Empty).Trim();
                
        return (cleanTitle, revision);

    }
    
    /// <summary>
    /// Extracts the clean application ID and version (if present) from the titleid field
    /// </summary>
    /// <param name="fullApplicationId">The application ID string potentially containing a version</param>
    /// <returns>A tuple containing (cleanApplicationId, version)</returns>
    public static (string CleanApplicationId, uint Version) ExtractApplicationIdAndVersion(string fullApplicationId)
    {
        if (string.IsNullOrEmpty(fullApplicationId))
            return (string.Empty, 0);

        var match = ApplicationIdPattern.Match(fullApplicationId);

        if (!match.Success) return (fullApplicationId.Trim(), 0);
        
        // Extract the clean application ID
        var cleanApplicationId = match.Groups[1].Value;
                
        // Extract the version if present
        uint version = 0;
        if (match.Groups[2].Success && uint.TryParse(match.Groups[2].Value, out var parsedVersion))
        {
            version = parsedVersion;
        }
                
        return (cleanApplicationId, version);
    }
    

    [GeneratedRegex(@"\s*\[Rev\s+([\d\.]+)\]\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex RevisionRegex();
    
    [GeneratedRegex(@"^([\dA-F]{16})\s*(?:\(v(\d+)\))?", RegexOptions.Compiled)]
    private static partial Regex VersionRegex();
}