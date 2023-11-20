using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Serilog;

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
    
    public static bool ClickOnElementByXPath(ChromeDriver chromeDriver, string xpath, bool ignoreLogForNoSuchElementEx = false)
    {
        try
        {
            var element = chromeDriver.FindElement(By.XPath(xpath));
            element.Click();
            return true;
        }
        catch (NoSuchElementException)
        {
            if (!ignoreLogForNoSuchElementEx)
                Log.Error("Could not found element with XPath: {Xpath}", xpath);
            return false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unhandled exception occurred while trying to click on element with xpath: {Xpath}", xpath);
            return false;
        }
    }

    public static bool ClickOnElementById(ChromeDriver chromeDriver, string id, bool ignoreLogForNoSuchElementEx = false)
    {
        try
        {
            var element = chromeDriver.FindElement(By.Id(id));
            element.Click();
            return true;
        }
        catch (NoSuchElementException)
        {
            if (!ignoreLogForNoSuchElementEx)
                Log.Error($"Could not found element with id: {id}", id);
            return false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unhandled exception occurred while trying to click on element with id: {Id}", id);
            return false;
        }
    }
    
    public static bool GoToNextFarmPage(ChromeDriver driver, int currentPage)
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