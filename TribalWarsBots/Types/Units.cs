using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace TribalWarsBots.Types;

public class Units
{
    #region Properties
    
    public int? Spear { get; init; }
    public int? Sword { get; init; }
    public int? Archer { get; init; }
    public int? Axe { get; init; }
    
    public int? Spy { get; init; }
    public int? Light { get; init; }
    public int? Marcher { get; init; }
    public int? Heavy { get; init; }
    
    public int? Ram { get; init; }
    public int? Catapult { get; init; }
    
    public int? Knight { get; init; }
    public int? Snob { get; init; }
    public int? Militia { get; init; }
    
    public int? UnitsSum { get; set; }
    
    #endregion
    
    #region Methods

    public static Units GetUnitNumbers(ChromeDriver chromeDriver, string tableId, int rowIndex)
    {
        var units = new Units
        {
            Spear = GetUnitValue(chromeDriver, tableId, "unit-item-spear", rowIndex),
            Sword = GetUnitValue(chromeDriver, tableId, "unit-item-sword", rowIndex),
            Archer = GetUnitValue(chromeDriver, tableId, "unit-item-archer", rowIndex),
            Axe = GetUnitValue(chromeDriver, tableId, "unit-item-axe", rowIndex),
            Spy = GetUnitValue(chromeDriver, tableId, "unit-item-spy", rowIndex),
            Light = GetUnitValue(chromeDriver, tableId, "unit-item-light", rowIndex),
            Marcher = GetUnitValue(chromeDriver, tableId, "unit-item-marcher", rowIndex),
            Heavy = GetUnitValue(chromeDriver, tableId, "unit-item-heavy", rowIndex),
            Ram = GetUnitValue(chromeDriver, tableId, "unit-item-ram", rowIndex),
            Catapult = GetUnitValue(chromeDriver, tableId, "unit-item-catapult", rowIndex),
            Knight = GetUnitValue(chromeDriver, tableId, "unit-item-knight", rowIndex),
            Snob = GetUnitValue(chromeDriver, tableId, "unit-item-snob", rowIndex),
            Militia = GetUnitValue(chromeDriver, tableId, "unit-item-militia", rowIndex),
        };
        
        var unitValues = new List<int?>
        {
            units.Spear,
            units.Sword,
            units.Archer,
            units.Axe,
            units.Spy,
            units.Light,
            units.Marcher,
            units.Heavy,
            units.Ram,
            units.Catapult,
            units.Knight,
            units.Snob,
            units.Militia,
        };

        if (unitValues.Exists(u => u.HasValue))
            units.UnitsSum = unitValues.Where(u => u.HasValue).Sum(u => u!.Value);
        else
            units.UnitsSum = null; 

        return units;
    }

    private static int? GetUnitValue(ChromeDriver chromeDriver, string tableId, string unitClass, int rowIndex)
    {
        try
        {
            var unitElement = chromeDriver.FindElement(By.CssSelector($"#{tableId} tr:nth-child({rowIndex + 1}) .{unitClass}"));

            if (string.IsNullOrWhiteSpace(unitElement.Text))
                return null;

            return int.TryParse(unitElement.Text, out var number) ? number : null;
        }
        catch (NoSuchElementException)
        {
            return null;
        }
    }
    
    #endregion
}