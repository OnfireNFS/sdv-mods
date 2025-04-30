using StardewModdingAPI;

namespace CompanionAdventures.Framework;

/// <summary>
/// This is just a convenience class that contains a collection of the different "Stores" used throughout this mod, this
/// just makes importing them a bit nicer since the imports are always Store.useThing.
/// </summary>
public static class Stores
{
    public static CompanionManager useCompanionManager()
    {
        return CompanionManager.UseCompanionManager();
    }
    
    public static CompanionAdventures useMod()
    {
        return CompanionAdventures.UseCompanionAdventures();
    }

    public static Multiplayer useMultiplayer()
    {
        return Multiplayer.UseMultiplayer();
    }
    
    /****
     * Shortcuts to common mod resources
     ****/
    public static ModConfig useConfig()
    {
        return useMod().Config;
    }
    public static IMonitor useMonitor()
    {
        return useMod().Monitor;
    }
}