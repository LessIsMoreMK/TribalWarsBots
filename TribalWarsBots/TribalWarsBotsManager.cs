using OpenQA.Selenium.Chrome;
using Serilog;
using TribalWarsBots.Bots;
using TribalWarsBots.Types;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace TribalWarsBots;

public class TribalWarsBotsManager
{
    #region Setup
    
    private readonly ChromeDriver _chromeDriver;
    private readonly BotSettings _botSettings;
    
    public TribalWarsBotsManager(BotSettings botSettings)
    {
        new DriverManager().SetUpDriver(new ChromeConfig());
        _chromeDriver = new ChromeDriver(new ChromeOptions
        {
            DebuggerAddress = "localhost:9222"
        });

        _botSettings = botSettings;
    }
    
    #endregion
    
    #region Methods
    
    public async Task RunBotsManager()
    {
        var timeForNextGatheringIteration = -1;
        var timeForNextFarmSimpleIteration = -1;
        var timeForNextFarmAdvancedIteration = -1;
        
        while (true)
        {
            var currentTime = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            if (_botSettings.GatheringBotEnabled && timeForNextGatheringIteration <= currentTime)
            {
                var tribalWarsGatheringBot = new TribalWarsGatheringBot(_chromeDriver, _botSettings.ChromeTabStartOfUrl);
                timeForNextGatheringIteration = currentTime + await tribalWarsGatheringBot.RunAsync();
            }
            if (_botSettings.SimpleFarmBotEnabled && timeForNextFarmSimpleIteration <= currentTime)
            {
                await new TribalWarsFarmBot(_chromeDriver, _botSettings.ChromeTabStartOfUrl, FarmMode.Simple, _botSettings.SimpleFarmSwitchPages).RunAsync();
                var nextSimpleFarmTimeInSeconds = new Random().Next(_botSettings.SimpleFarmIntervalMin, _botSettings.SimpleFarmIntervalMax) * 60;
                timeForNextFarmSimpleIteration = currentTime + nextSimpleFarmTimeInSeconds;
            }
            if (_botSettings.AdvancedFarmBotEnabled && timeForNextFarmAdvancedIteration <= currentTime)
            {
                await new TribalWarsFarmBot(_chromeDriver, _botSettings.ChromeTabStartOfUrl, FarmMode.Advanced, _botSettings.SimpleFarmSwitchPages).RunAsync();
                var nextAdvancedFarmTimeInSeconds = new Random().Next(_botSettings.AdvancedFarmIntervalMin, _botSettings.AdvancedFarmIntervalMax) * 60;
                timeForNextFarmAdvancedIteration = currentTime + nextAdvancedFarmTimeInSeconds;
            }
            
            var times = new int[] { timeForNextGatheringIteration, timeForNextFarmSimpleIteration, timeForNextFarmAdvancedIteration };
            AdjustBotTimings(ref times);

            timeForNextGatheringIteration = times[0];
            timeForNextFarmSimpleIteration = times[1];
            timeForNextFarmAdvancedIteration = times[2];
            
            if (timeForNextGatheringIteration > currentTime)
                Log.Information($"Next TribalWarsGatheringBot iteration at: {TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow.Add(TimeSpan.FromSeconds
                    (timeForNextGatheringIteration - currentTime)), TimeZoneInfo.Local)}");
            if (timeForNextFarmSimpleIteration > currentTime)
                Log.Information($"Next Simple Farm Bot iteration at: {TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow.Add(TimeSpan.FromSeconds(timeForNextFarmSimpleIteration - currentTime)), TimeZoneInfo.Local)}");
            if (timeForNextFarmAdvancedIteration > currentTime)
                Log.Information($"Next Advanced Farm Bot iteration at: {TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow.Add(TimeSpan.FromSeconds(timeForNextFarmAdvancedIteration - currentTime)), TimeZoneInfo.Local)}");

            var nextIterationTimes = new List<int> { timeForNextGatheringIteration, timeForNextFarmSimpleIteration, timeForNextFarmAdvancedIteration }
                .Where(t => t > currentTime)
                .DefaultIfEmpty(-1)
                .Min();

            if (nextIterationTimes > 0)
            {
                var delay = nextIterationTimes - currentTime;
                await Task.Delay(TimeSpan.FromSeconds(delay));
            }
            else
                return;
        }
    }
    
    #endregion
    
    #region Private Helpers
    
    /// <summary>
    /// When times overlap in two minutes range adjust them to not run bots in same time. 
    /// </summary>
    private static void AdjustBotTimings(ref int[] times)
    {
        var validTimes = times.Select((time, index) => (time, index))
            .Where(t => t.time > 0)
            .OrderBy(t => t.time)
            .ToList();

        for (var i = 1; i < validTimes.Count; i++)
            if (validTimes[i].time - validTimes[i - 1].time < 120)
                validTimes[i] = (validTimes[i - 1].time + 120, validTimes[i].index);

        foreach (var (time, index) in validTimes)
            times[index] = time;
    }
    
    #endregion
}