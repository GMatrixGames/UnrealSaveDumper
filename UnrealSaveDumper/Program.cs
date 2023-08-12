global using Serilog;
using CUE4Parse.FileProvider;
using CUE4Parse.MappingsProvider;
using Newtonsoft.Json;
using Spectre.Console;
using UnrealSaveDumper;
using UnrealSaveDumper.GameSpecific;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console()
    .CreateLogger();

startOver:

var savFile = AnsiConsole.Ask<string>(@"Path to [dodgerblue2].sav[/] file to dump. [grey]ex. C:\CoolGame\Saved\SaveGames\SaveFile.sav[/]").Replace("\"", "");

while (string.IsNullOrWhiteSpace(savFile) || !File.Exists(savFile))
{
    savFile = AnsiConsole.Ask<string>(@"Path to [dodgerblue2].sav[/] file to dump. [grey]ex. C:\CoolGame\Saved\SaveGames\SaveFile.sav[/]");
}

var gameName = AnsiConsole.Prompt(new SelectionPrompt<string>()
    .Title("What is the [dodgerblue2]internal name[/] of the game?")
    .PageSize(10)
    .MoreChoicesText("[grey](Move up and down to find more options)[/]")
    .AddChoices("FortniteGame", "Palia", "Other (Manual)"));

if (gameName == "Other (Manual)")
{
    gameName = AnsiConsole.Ask<string>("What is the [dodgerblue2]internal name[/] of the game? [grey]ex. ShooterGame, Lyra, etc.[/]");

    while (string.IsNullOrWhiteSpace(gameName))
    {
        gameName = AnsiConsole.Ask<string>("What is the [dodgerblue2]internal name[/] of the game? [grey]ex. ShooterGame, Lyra, etc.[/]");
    }
}

AnsiConsole.WriteLine($"Selected game name is: {gameName}");

var fileName = Path.GetFileName(savFile);
var emptyProvider = new DefaultFileProvider(Directory.GetCurrentDirectory(), SearchOption.TopDirectoryOnly, true);
var mappingsPath = AnsiConsole.Ask("Path to a [blue]mapping (.usmap)[/] (if applicable).", string.Empty).Replace("\"", "");

if (!string.IsNullOrWhiteSpace(mappingsPath)) emptyProvider.MappingsContainer = new FileUsmapTypeMappingsProvider(mappingsPath);

var data = File.ReadAllBytes(savFile);
var saveGame = gameName != "FortniteGame" ? new FSaveGame(data, emptyProvider, gameName, fileName) : new FFortSaveGame(data, emptyProvider, fileName);

AnsiConsole.WriteLine($"Engine Version: {saveGame.Header.SavedEngineVersion}");
AnsiConsole.WriteLine($"Save Game Class: {saveGame.Header.SaveGameClassName}");

var jsonDumpPath = AnsiConsole.Ask<string>("Where would you like to save the json serialized version?").Replace("\"", "");

File.WriteAllText(jsonDumpPath, JsonConvert.SerializeObject(saveGame, Formatting.Indented));

if (AnsiConsole.Confirm("Dump another?"))
{
    goto startOver;
}

return 0;