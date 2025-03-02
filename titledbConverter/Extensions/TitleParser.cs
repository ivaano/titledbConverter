using System.Text;
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

    // Regex pattern used to remove revision with parenthesis in the title (rev001)
    private static readonly Regex RevisionPatternParenthesis = RevisionNoParenthesisRegex();

    // Regex pattern for partial ids
    // Matches "0100-20500C8C8000(v65536),-A1A00C5D8000(v131072)"
    private static readonly Regex MultiplePartialTitleIds = MultiplePartialTitleIdsRegex();
    
    // Regex for multiple title Ids
    // Matches "0100AAA00ACBE000 (v196608) + 010076D00E4BA000 (v65536)"
    private static readonly Regex MultipleTitleIds = MultipleTitleIdsRegex();
    
    /// <summary>
    /// Extracts the base title and revision (if present) from a full title string
    /// </summary>
    /// <param name="fullTitle">The full title string potentially containing a revision</param>
    /// <returns>A tuple containing (cleanTitle, revision)</returns>
    public static (string CleanTitle, string? Revision) ExtractTitleAndRevision(string fullTitle)
    {
        if (string.IsNullOrEmpty(fullTitle))
            return (string.Empty, null);

        var noRevParenthesisTitle = RevisionPatternParenthesis.Replace(fullTitle, string.Empty).Trim();

        var match = RevisionPattern.Match(fullTitle);

        if (!match.Success)
        {
            //remove remaining [.*] in titleName
            var cleanTitleName = Regex.Replace(noRevParenthesisTitle, @"\[.*?\]", "").Trim();    
            return (cleanTitleName, null);
        }
        var revision = match.Groups[1].Value;
        var cleanTitle = RevisionPattern.Replace(noRevParenthesisTitle, string.Empty).Trim();
            
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
    
    
    public static List<(string ApplicationId, uint Version)> ExtractTitleIds(string titleIdsString)
    {
        var results = new List<(string ApplicationId, uint Version)>();

        if (string.IsNullOrEmpty(titleIdsString))
        {
            return results;
        }

        if (Regex.IsMatch(titleIdsString, @"^[0-9]{4}-")) 
        {
            // Handle the case with 4 digits and hyphen
            var prefix = titleIdsString.Substring(0, 4);
            var remainingIds = titleIdsString.Substring(5);

            var matches = MultiplePartialTitleIds.Matches(remainingIds);

            foreach (Match match in matches)
            {
                var titleId = prefix + match.Groups[1].Value;
                var version = Convert.ToUInt32(match.Groups[2].Value);
                results.Add((titleId, version));
            }
        }
        else
        {
            // Handle the case with plus signs
            var matches = MultipleTitleIds.Matches(titleIdsString);

            foreach (Match match in matches)
            {
                var titleId = match.Groups[1].Value;
                var version = Convert.ToUInt32(match.Groups[2].Value);
                results.Add((titleId, version));
            }
        }

        return results;
    }
    
   

    [GeneratedRegex(@"\s*\[Rev\s+([\d\.]+)\]\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex RevisionRegex();
    
    [GeneratedRegex(@"^([\dA-F]{16})\s*(?:\(v(\d+)\))?", RegexOptions.Compiled)]
    private static partial Regex VersionRegex();
    
    [GeneratedRegex(@"\(rev(\d+)\)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex RevisionNoParenthesisRegex();
    
    [GeneratedRegex(@"([A-Fa-f0-9]{12})\s*\(v(\d+)\)")]
    private static partial Regex MultiplePartialTitleIdsRegex();
    
    [GeneratedRegex(@"([A-Fa-f0-9]{16})\s*\(v(\d+)\)")]
    private static partial Regex MultipleTitleIdsRegex();
}