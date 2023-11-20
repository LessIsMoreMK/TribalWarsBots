using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using Serilog;
using TribalWarsBots.Helpers;
using TribalWarsBots.Types;

namespace TribalWarsBots.Bots;

public class TribalWarsFarmBot
{
    #region Setup
        
    private readonly ChromeDriver _chromeDriver;
    private readonly FarmMode _farmMode;
    private readonly bool _switchPages;
    private readonly string _chromeTabStartOfUrl;
        
    public TribalWarsFarmBot(ChromeDriver chromeDriver, string chromeTabStartOfUrl, FarmMode farmMode, bool switchPages)
    {
        _chromeDriver = chromeDriver;
        _chromeTabStartOfUrl = chromeTabStartOfUrl;
        _farmMode = farmMode;
        _switchPages = switchPages;
    }

    #endregion
    
    #region Methods

    public async Task RunAsync()
    {
        var currentPage = 0;
        var sendAttacks = 0;
        var alertHandled = false;
        
        try
        {
            var switchToTabResult = NavigationHelpers.SwitchToTabWithUrl(_chromeDriver, _chromeTabStartOfUrl);
            if (!switchToTabResult)
                return;

            (_chromeDriver).ExecuteScript("window.scrollTo(0, 0);");
            await Task.Delay(new Random().Next(100, 300));
            NavigationHelpers.ClickOnElementById(_chromeDriver, "manager_icon_farm");
            await Task.Delay(new Random().Next(300, 800));
            
            do
            {
                await Task.Delay(new Random().Next(300, 800));
                var farmRows = _chromeDriver.FindElements(By.XPath("//tr[starts-with(@id, 'village_') and contains(@class, 'report_')]"));
                foreach (var farmRow in farmRows)
                {
                    try
                    {
                        (_chromeDriver).ExecuteScript("arguments[0].scrollIntoView({behavior: 'smooth', block: 'center'});", farmRow);
                        
                        if (farmRow.FindElements(By.CssSelector(".farm_icon_disabled")).Any() && _farmMode == FarmMode.Simple)
                            break;

                        if (!await ProcessFarmRow(farmRow))
                            continue;

                        sendAttacks++;
                        
                        alertHandled = AlertsHandler.CheckForErrors(_chromeDriver);
                        if (alertHandled && _farmMode == FarmMode.Simple)
                            break;
                        
                        await Task.Delay(new Random().Next(251, 300));
                    }
                    catch (Exception ex)
                    {
                        //Ignore row
                    }
                }
                currentPage++;
            } while (_switchPages && 
                     (!alertHandled || _farmMode == FarmMode.Advanced) && 
                     NavigationHelpers.GoToNextFarmPage(_chromeDriver, currentPage));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unhandled exception occurred while running TribalWarsFarmBot in mode {FarmMode}", _farmMode);
        }
        
        Log.Information("TribalWarsFarmBot in mode {FarmMode} stopped at page {CurrentPage} send: {SendAttacks} attacks.", _farmMode, currentPage, sendAttacks);
    }
        
    #endregion
        
    #region Private Helpers
    
    private async Task<bool> ProcessFarmRow(IWebElement row)
    {
        try
        {
            var statusImages = row.FindElements(By.TagName("img"));
            var imageSrc = statusImages[1].GetAttribute("src");
            var wallLevel = GetWallLevel(row);
            
            if (imageSrc.Contains("graphic/dots/green.png"))
                return await HandleGreenAttack(row, wallLevel);
            
            if (_farmMode == FarmMode.Advanced && !imageSrc.Contains("graphic/dots/green.png"))
                return await HandleNonGreenAttack(row, wallLevel);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error processing farm row");
        }

        return false;
    }
    
    private async Task<bool> HandleGreenAttack(IWebElement row, string? wallLevel)
    {
        try
        {
            switch (wallLevel)
            {
                case "?" or "0" when _farmMode == FarmMode.Simple:
                    _chromeDriver.ExecuteScript("arguments[0].click();", row.FindElement(By.ClassName("farm_icon_b")));
                    return true;
                
                case not "?" or "0" when _farmMode == FarmMode.Advanced:
                    return await HandleNonGreenAttack(row, wallLevel);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while handing green attack");
        }
        
        return false;
    }
    
    private async Task<bool> HandleNonGreenAttack(IWebElement row, string? wallLevel)
    {
        try
        {
            var units = GetReportInfo(row);

            if (wallLevel == "0" && units.Defender.UnitsSum == 0)
            {
                _chromeDriver.ExecuteScript("arguments[0].click();", row.FindElement(By.ClassName("farm_icon_b")));
                return true;
            }
            
            if (units.Defender.UnitsSum == 0)
                return await SendRams(row, wallLevel);
            
            if (units.Agresor.Spy == 0 && units.AgresorLost.Spy == 0)
                return await SendScout(row);
            
            //Left rest for user handling
            return false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while handling non green attack");
        }

        return false;
    }

    private async Task<bool> SendScout(IWebElement row)
    {
        try
        {
            if (!await OpenPlace(row))
                return false;

            var spyInput = _chromeDriver.FindElement(By.Id("unit_input_spy"));
            spyInput.Clear();
            spyInput.SendKeys("1");

            if (!await ConfirmAttack())
                return false;
            
            if (AlertsHandler.CheckForErrors(_chromeDriver))
                new Actions(_chromeDriver).SendKeys(Keys.Escape).Perform();
            else
                DeleteReport(row);

            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while sending scout");
        }

        return false;
    }

    private async Task<bool> SendRams(IWebElement row, string? wallLevel)
    {
        try
        {
            if (!await OpenPlace(row))
                return false;

            if (!InputUnitsBasedOnWallLevel(wallLevel))
                return false;

            if (!await ConfirmAttack())
                return false;
            
            if (AlertsHandler.CheckForErrors(_chromeDriver))
                new Actions(_chromeDriver).SendKeys(Keys.Escape).Perform();
            else
                DeleteReport(row);

            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while sending rams");
        }

        return false;
    }

    private bool InputUnitsBasedOnWallLevel(string? wallLevel)
    {
        try
        {
            var spyInput = _chromeDriver.FindElement(By.Id("unit_input_spy"));
            spyInput.Clear();
            spyInput.SendKeys("1");
            
            var axesInput = _chromeDriver.FindElement(By.Id("unit_input_axe"));
            axesInput.Clear();
            axesInput.SendKeys("100");
                    
            var lightInput = _chromeDriver.FindElement(By.Id("unit_input_light"));
            lightInput.Clear();
            lightInput.SendKeys("4");

            switch (wallLevel)
            {
                case "1":
                {
                    var ramInput = _chromeDriver.FindElement(By.Id("unit_input_ram"));
                    ramInput.Clear();
                    ramInput.SendKeys("2");
                    break;
                }
                case "2":
                {
                    var ramInput = _chromeDriver.FindElement(By.Id("unit_input_ram"));
                    ramInput.Clear();
                    ramInput.SendKeys("4");
                    break;
                }
                case "3":
                {
                    var ramInput = _chromeDriver.FindElement(By.Id("unit_input_ram"));
                    ramInput.Clear();
                    ramInput.SendKeys("8");
                    break;
                }
                default:
                    return false; //Left rest of user handling
            }

            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during input units");
        }

        return false;
    }

    private async Task<bool> OpenPlace(IWebElement row)
    {
        try
        {
            var element = row.FindElement(By.XPath(".//a[.//img[contains(@src, 'graphic/buildings/place.png')]]"));
            element.Click();
            
            await Task.Delay(new Random().Next(300, 800));
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while opening place");
        }

        return false;
    }

    private void DeleteReport(IWebElement row)
    {
        try
        {
            var deleteReportsLink = row.FindElement(By.XPath(".//a[.//img[contains(@src, 'graphic/delete_small.png')]]"));
            deleteReportsLink.Click();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while deleting report");
        }
    }

    private async Task<bool> ConfirmAttack()
    {
        try
        {
            if (!NavigationHelpers.ClickOnElementByXPath(_chromeDriver, "//form[@id='command-data-form']//input[@type='submit']"))
                return false;
            
            await Task.Delay(new Random().Next(800, 1300));
            
            if (!NavigationHelpers.ClickOnElementById(_chromeDriver, "troop_confirm_submit"))
                return false;
            
            await Task.Delay(new Random().Next(800, 1300));
            
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while confirming attack");
        }

        return false;
    }

    private static string? GetWallLevel(IWebElement row)
    {
        try
        {
            var valueTds = row.FindElements(By.XPath(".//td[@style='text-align: center;']"));
            return valueTds.Any() ? valueTds[^1].Text.Trim() : null; 
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting wall value");
            return null;
        }
    }
    
    private (Units Agresor, Units AgresorLost, Units Defender, Units DefenderLost) GetReportInfo(ISearchContext row)
    {
        var originalWindow = _chromeDriver.CurrentWindowHandle;
        
        var reportLink = row.FindElement(By.XPath(".//td/a[contains(@href, 'screen=report')]")).GetAttribute("href");
        ((IJavaScriptExecutor)_chromeDriver).ExecuteScript("window.open(arguments[0]);", reportLink);
        var windows = _chromeDriver.WindowHandles;
        _chromeDriver.SwitchTo().Window(windows[^1]);
        
        var aggressorSent = Units.GetUnitNumbers(_chromeDriver, "attack_info_att_units", 1); 
        var aggressorLost = Units.GetUnitNumbers(_chromeDriver, "attack_info_att_units", 2); 
        var defender = Units.GetUnitNumbers(_chromeDriver, "attack_info_def_units", 1); 
        var defenderLost = Units.GetUnitNumbers(_chromeDriver, "attack_info_def_units", 2);

        _chromeDriver.Close();
        _chromeDriver.SwitchTo().Window(originalWindow);

        return (aggressorSent, aggressorLost, defender, defenderLost);
    }

    #endregion
}