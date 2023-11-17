using Microsoft.Extensions.Configuration;
using Serilog;
using TribalWarsBots;
using TribalWarsBots.Types;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var seqConfig = configuration.GetSection("seq");

if (seqConfig.GetValue<bool>("enabled"))
{
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .WriteTo.Seq(seqConfig.GetValue<string>("url") ?? throw new InvalidOperationException())
        .CreateLogger();
}

var botSettings = configuration.GetSection("farmBotSettings").Get<BotSettings>();

try
{
    Log.Information("");
    Log.Information("Starting up the TribalWarsBots application");
    var tribalWarsBotsManager = new TribalWarsBotsManager(botSettings!);
    await tribalWarsBotsManager.RunBotsManager();
}
catch (Exception ex)
{
    Log.Error(ex, "An unhandled exception occurred.");
}
finally
{
    Log.CloseAndFlush();
}







