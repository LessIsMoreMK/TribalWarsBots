using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Serilog;
using TribalWarsBots.Types;

namespace TribalWarsBots.Helpers;

public static class NavigationHelpers
{
    #region Methods
    
    public static bool SwitchToTabWithUrl(ChromeDriver chromeDriver, string tabStartOfUrl)
    {
        foreach (var handle in chromeDriver.WindowHandles)
        {
            chromeDriver.SwitchTo().Window(handle);
            if (chromeDriver.Url.StartsWith(tabStartOfUrl))
                return true;
        }

        Log.Error("Tab with start of url {TabStartOfUrl} not found", tabStartOfUrl);
        return false;
    }
    
    public static bool ClickOnElementByXPath(ChromeDriver chromeDriver, string xpath, bool ignoreNoSuchElementEx = false)
    {
        try
        {
            var element = chromeDriver.FindElement(By.XPath(xpath));
            element.Click();
            return true;
        }
        catch (NoSuchElementException)
        {
            if (!ignoreNoSuchElementEx)
                Log.Error("Could not found element with XPath: {Xpath}", xpath);

            return false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unhandled exception occurred while trying to click on xpath: {Xpath}", xpath);
            return false;
        }
    }

    public static bool ClickOnElementById(ChromeDriver chromeDriver, string id, bool ignoreNoSuchElementEx = false)
    {
        try
        {
            var elementToClick = chromeDriver.FindElement(By.Id(id));
            elementToClick.Click();
            return true;
        }
        catch (NoSuchElementException)
        {
            if (!ignoreNoSuchElementEx)
                Log.Error($"Could not found element with id: {id}", id);
            return false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unhandled exception occurred while trying to click on element with id: {Id}", id);
            return false;
        }
    }
    
    public static bool GoToNextFarmPage(ChromeDriver driver, int currentPage, FarmMode farmMode)
    {
        try
        {
            (driver).ExecuteScript("window.scrollTo(0, 0);");
            var xpath = $"//div[@id='plunder_list_nav']//a[@class='paged-nav-item' and contains(@href, 'Farm_page={currentPage}')]";
            var nextPageLink = driver.FindElement(By.XPath(xpath));
            nextPageLink.Click();
            return true;
        }
        catch (NoSuchElementException)
        {
            Log.Information("TribalWarsFarmBot in mode {FarmMode} stopped at page {CurrentPage}", farmMode, currentPage);
            return false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unhandled exception occurred while trying to GoToNextFarmPage");
            return false;
        }
    }
    
    #endregion
}