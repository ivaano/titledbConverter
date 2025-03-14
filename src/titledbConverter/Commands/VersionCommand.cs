using System.Reflection;
using Spectre.Console;
using Spectre.Console.Cli;

namespace titledbConverter.Commands;

public class VersionCommand : AsyncCommand
{

    public override Task<int> ExecuteAsync(CommandContext context)
    {
        var assembly = Assembly.GetEntryAssembly(); // Or Assembly.GetCallingAssembly() in some cases
        if (assembly != null) {
            var version = assembly.GetName().Version;
            AnsiConsole.MarkupLine($"[green]{assembly.GetName().Name} Version {version}[/]");
        } else {
            AnsiConsole.MarkupLine("[red]Could not determine version.[/]");
        }

        return Task.FromResult(0);
    }
}