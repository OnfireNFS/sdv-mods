using CompanionAdventures.Framework.Models;
using StardewModdingAPI;

namespace CompanionAdventures.Framework;

public partial class Store(CompanionAdventures mod)
{
    private ModConfig _config = mod.Helper.ReadConfig<ModConfig>();
    
    // returns the config for this instance of mod
    public ModConfig UseConfig()
    {
        return _config;
    }

    public IModHelper UseHelper()
    {
        return mod.Helper;
    }

    public IManifest UseManifest()
    {
        return mod.ModManifest;
    }
    
    public IMonitor UseMonitor()
    {
        return mod.Monitor;
    }
}