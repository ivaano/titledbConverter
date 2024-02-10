using System.Collections.Concurrent;
using System.Text.Json;
using Spectre.Console;
using titledbConverter.Models;
using titledbConverter.Models.Dto;
using titledbConverter.Services.Interface;
using Version = titledbConverter.Models.Version;

namespace titledbConverter.Services;

public class TitleDbService(IDbService dbService) : ITitleDbService
{
    public async Task ImportRegionAsync(string regionFile)
    {
        AnsiConsole.MarkupLineInterpolated($"[bold green]Processing {regionFile}[/]");
        

        Dictionary<string, TitleDbTitle> games;
        //Dictionary<string, TitleDbVersions> versions;
        ConcurrentDictionary<string, ConcurrentDictionary<string, TitleDbCnmt>> concurrentCnmts;
        ConcurrentDictionary<string, TitleDbVersions> concurrentVersions;
        
        AnsiConsole.MarkupLine($"[springgreen3_1]Loading Versions...[/]");
        await using (var stream = File.OpenRead("I:\\titledb\\versions.json"))
        {
            var versions = await JsonSerializer.DeserializeAsync<Dictionary<string, TitleDbVersions>>(stream);
            concurrentVersions = new ConcurrentDictionary<string, TitleDbVersions>(
                versions.ToDictionary(
                    kvp => kvp.Key.ToUpper(), 
                    kvp => kvp.Value
                )
            );
        } 
        
        AnsiConsole.MarkupLine($"[springgreen3_1]Loading Cnmts...[/]");
        await using (var stream = File.OpenRead("I:\\titledb\\cnmts.json"))
        {
            var cnmts = await JsonSerializer.DeserializeAsync<Dictionary<string, Dictionary<string, TitleDbCnmt>>>(stream);
            concurrentCnmts = new ConcurrentDictionary<string, ConcurrentDictionary<string, TitleDbCnmt>>(
                cnmts.ToDictionary(
                    kvp => kvp.Key.ToUpper(), 
                    kvp => new ConcurrentDictionary<string, TitleDbCnmt>(kvp.Value)
                )
            );
        } 
        
        AnsiConsole.MarkupLine($"[springgreen3_1]Loading Titles...[/]");
        await using (var stream = File.OpenRead(regionFile))
        {
            games = await JsonSerializer.DeserializeAsync<Dictionary<string, TitleDbTitle>>(stream) ?? throw new InvalidOperationException();
        }

        var titles = new ConcurrentBag<Title>();
        var options = new ParallelOptions { MaxDegreeOfParallelism = 1 };

        Parallel.ForEach(games, options, game =>
        {
            var title = new Title
            {
                NsuId = game.Value.NsuId,
                ApplicationId = game.Value.Id,
                TitleName = game.Value.Name,
                Cnmts = new List<Cnmt>(),
                Versions = new List<Version>()
            };

            if (!string.IsNullOrEmpty(game.Value.Id))
            {
                //DLC
                if (concurrentCnmts.ContainsKey(game.Value.Id))
                {
                    var cnmt = concurrentCnmts[game.Value.Id];
                
                    foreach (var (key, value) in cnmt)
                    {
                        var titleCnmt = new Cnmt
                        {
                            OtherApplicationId = value.OtherApplicationId,
                            RequiredApplicationVersion = value.RequiredApplicationVersion,
                            TitleType = value.TitleType,
                            Version = value.Version
                        };
                        title.Cnmts.Add(titleCnmt);
                    }
                }
                
                //Updates
                if (concurrentVersions.TryGetValue(game.Value.Id, out var vers))
                {
                    foreach (var version in vers)
                    {
                        var titleVersion = new Version
                        {
                            VersionNumber = Convert.ToInt32(version.Key),
                            VersionDate = version.Value,
                            Title = title
                        };
                        title.Versions.Add(titleVersion);
                    }
                }
                
            }
            titles.Add(title);
        });
        

        
        await dbService.BulkInsertTitlesAsync(titles.ToList());
        /*
        foreach (var title in titles)
        {
            await dbService.AddTitleAsync(title);
        }
        */
        titles.Clear();
        AnsiConsole.MarkupLine($"[lightslateblue]Title Count {games.Count}[/]");
    
        //var json = await File.ReadAllTextAsync(regionFile);
        //var games = JsonSerializer.Deserialize<Dictionary<string, TitleDbTitle>>(json);
        var pepa = "hola";
    }
}