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
                timeForNextGatheringIteration = currentTime + await new TribalWarsGatheringBot(_chromeDriver, _botSettings.ChromeTabStartOfUrl).RunAsync();
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

            if (await ScheduleNextIterationsAndLog(new int[] {timeForNextGatheringIteration, timeForNextFarmSimpleIteration, timeForNextFarmAdvancedIteration}, currentTime))
                return;
        }
    }
    
    #endregion
    
    #region Private Helpers
    
    private static async Task<bool> ScheduleNextIterationsAndLog(int[] times, int currentTime)
    {
        AdjustBotTimings(ref times);
           
        if (times[0] > currentTime) // TribalWarsGatheringBot
            Log.Information($"Next TribalWarsGatheringBot iteration at: {TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(times[0]).DateTime, TimeZoneInfo.Local)}");
        if (times[1] > currentTime) // TribalWarsSimpleFarmBot
            Log.Information($"Next TribalWarsSimpleFarmBot iteration at: {TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(times[1]).DateTime, TimeZoneInfo.Local)}");
        if (times[2] > currentTime) // TribalWarsAdvancedFarmBot
            Log.Information($"Next TribalWarsAdvancedFarmBot iteration at: {TimeZoneInfo.ConvertTimeFromUtc(DateTimeOffset.FromUnixTimeSeconds(times[2]).DateTime, TimeZoneInfo.Local)}");

        var nearestTime = times.Where(t => t > currentTime).DefaultIfEmpty(-1).Min();
        if (nearestTime <= 0) 
            return true;
        
        await Task.Delay(TimeSpan.FromSeconds(nearestTime - currentTime));
        return false;
    }
    
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