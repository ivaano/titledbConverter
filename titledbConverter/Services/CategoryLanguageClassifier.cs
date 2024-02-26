using System.Collections.Immutable;
using NetTopologySuite.Index.Bintree;
using titledbConverter.Services.Interface;

namespace titledbConverter.Services;

public class CategoryLanguageClassifier : ICategoryLanguageClassifier
{

    private readonly ImmutableHashSet<string> _knownCategories = ImmutableHashSet.Create("Action", "Adventure", "Arcade", "Board Game", "Communication", "Education", "Fighting", "First-Person Shooter", "Lifestyle", "Multiplayer", "Music", "Other", "Party", "Platformer", "Practical", "Puzzle", "Racing", 
        "RPG", "Shooter", "Simulation", "Sports", "Strategy", "Study", "Training", "Updates", "Utility", "Video");
    
    private readonly Dictionary<string, string> _jpjaMap = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
    {
        {"アクション", "Action"}
    };
    
    
    public CategoryLanguageClassifier()
    {
    }
    
    public Task ClassifyCategoryLanguageAsync(string region, string language, string name)
    {
        if (region == "JP" && language == "ja")
        {
            if (name.Contains("アクション"))
            {
                return Task.CompletedTask;
            }
        } 
    }
}