using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TribalWarsBots.Helpers;
using Log = Serilog.Log;

namespace TribalWarsBots.Bots;

public class TribalWarsGatheringBot
{
    #region Setup
        
    private readonly ChromeDriver _chromeDriver;
    private readonly string _chromeTabStartOfUrl;
    private readonly List<string> _gatheringLevels = new()
    {
        "Specjaliści surowcowi",
        "Zawodowi zbieracze",
        "Cierpliwi ciułacze",
        "Ambitni amatorzy"
    };
        
    public TribalWarsGatheringBot(ChromeDriver chromeDriver, string chromeTabStartOfUrl)
    {
        _chromeDriver = chromeDriver;
        _chromeTabStartOfUrl = chromeTabStartOfUrl;
    }

    #endregion
        
    #region Methods

    public async Task<int> RunAsync()
    {
        try
        {
            var switchToTabResult = NavigationHelpers.SwitchToTabWithUrl(_chromeDriver, _chromeTabStartOfUrl);
            if (!switchToTabResult)
                return -1;

            (_chromeDriver).ExecuteScript("window.scrollTo(0, 0);");
            NavigationHelpers.ClickOnElementByXPath(_chromeDriver, "//a[@class='quickbar_link' and @data-hash='cc5ffd9297792b3360ffc14dba7edf5f']");
            await Task.Delay(new Random().Next(300, 800));
            
            NavigationHelpers.ClickOnElementByXPath(_chromeDriver, "//a[contains(@href, 'screen=place') and contains(@href, 'mode=scavenge')]");
            await Task.Delay(new Random().Next(300, 800));

            foreach (var level in _gatheringLevels)
                if (await GatherResourcesForLevel(level))
                    Log.Information("TribalWarsGatheringBot - Gathered on level: {Level}", level);

            return GetTimeForNextGatheringIteration();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unhandled exception occurred while running TribalWarsGatheringBot");
        }
        
        return -1;
    }
        
    #endregion
        
    #region Private Helpers
    
    private async Task<bool> GatherResourcesForLevel(string levelTitle)
    {
        await Task.Delay(new Random().Next(300, 800));
        
        (_chromeDriver).ExecuteScript("window.scrollTo(0, 0);");
        await Task.Delay(new Random().Next(100, 300));
        NavigationHelpers.ClickOnElementByXPath(_chromeDriver, "//a[@data-title='Zbierak']");
        await Task.Delay(new Random().Next(300, 800));
        
        AlertsHandler.HandleAlert(_chromeDriver);
        
        _chromeDriver.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
        await Task.Delay(new Random().Next(100, 300));
        
        return NavigationHelpers.ClickOnElementByXPath(_chromeDriver,  
            $"//div[contains(@class, 'scavenge-option') and div[@class='title' and contains(text(), " +
            $"'{levelTitle}')]]//a[contains(@class, 'btn') and contains(@class, 'free_send_button')]", true);
    }

    private int GetTimeForNextGatheringIteration()
    {
        var gatheringCountdownElement = _chromeDriver.FindElements(By.CssSelector(".scavenge-option .return-countdown"));
        var longestTime = 0;

        foreach (var gatheringCountDownText in gatheringCountdownElement.Select(element => element.Text))
        {
            var timeParts = gatheringCountDownText.Split(':');
            var seconds = int.Parse(timeParts[0]) * 3600 + int.Parse(timeParts[1]) * 60 + int.Parse(timeParts[2]);
            longestTime = Math.Max(longestTime, seconds);
        }

        return longestTime + new Random().Next(45, 75); // Add 45-75 seconds
     }
        
    #endregion
}