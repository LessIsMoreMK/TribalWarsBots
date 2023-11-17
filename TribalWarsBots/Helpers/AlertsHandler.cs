using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Serilog;

namespace TribalWarsBots.Helpers;

public static class AlertsHandler
{
    public static void HandleAlert(IWebDriver driver)
    {
        try
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(0.5));
            wait.Until(drv =>
            {
                try
                {
                    drv.SwitchTo().Alert();
                    return true;
                }
                catch (NoAlertPresentException)
                {
                    return false;
                }
            });
            
            var alert = driver.SwitchTo().Alert();
            var alertText = alert.Text; 
            if (alertText.Contains("Brak dostępnych poziomów zbieractwa"))
                alert.Accept();
        }
        catch (WebDriverTimeoutException)
        {
            //Ignore
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An unhandled exception occurred while handling alert");
        }
    }
    
    public static bool CheckForErrors(IWebDriver driver)
    {
        var errorElements = driver.FindElements(By.XPath("//div[@class='autoHideBox error']/p"));
        foreach (var errorElement in errorElements)
        {
            if (errorElement.Text.Contains("Nie masz wystarczającej liczby jednostek"))
            {
                Log.Warning("Stopping iteration due to: 'Nie masz wystarczającej liczby jednostek'");
                return true;
            }
        }
        
        return false;
    }
}