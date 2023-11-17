namespace TribalWarsBots.Types;

public class BotSettings
{
    public string ChromeTabStartOfUrl { get; set; } = null!;

    public bool GatheringBotEnabled { get; set; }
    
    
    public bool SimpleFarmBotEnabled { get; set; }
    public bool SimpleFarmSwitchPages { get; set; }
    public int SimpleFarmIntervalMin { get; set; }
    public int SimpleFarmIntervalMax { get; set; }
    
    
    public bool AdvancedFarmBotEnabled { get; set; }
    
    public bool AdvancedFarmSwitchPages { get; set; }
    public int AdvancedFarmIntervalMin { get; set; }
    public int AdvancedFarmIntervalMax { get; set; }
}