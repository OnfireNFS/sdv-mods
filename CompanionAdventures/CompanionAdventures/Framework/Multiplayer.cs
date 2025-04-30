using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace CompanionAdventures.Framework;

public class Multiplayer
{
    private static Multiplayer? _instance = null;
    
    /// <summary>
    /// Get a reference to this store
    /// </summary>
    /// <returns>An instance of the CompanionManager store</returns>
    public static Multiplayer UseMultiplayer()
    {
        _instance ??= new Multiplayer();

        return _instance;
    }
    
    public void ReceiveMessage(ModMessageReceivedEventArgs e)
    {
        IMonitor monitor = Stores.useMonitor();
        
        string data = e.ReadAs<string>();
        monitor.Log($"Received \"{e.Type}\" event with data: {data}", LogLevel.Trace);
    }
    
    public void SendMessage(string message)
    {
        CompanionAdventures mod = Stores.useMod();
        IManifest modManifest = mod.ModManifest;
        IMultiplayerHelper multiplayer = mod.Helper.Multiplayer;
        
        string data = "Test Data";
        
        multiplayer.SendMessage(data, "companionadventures.companion.add", new []{ modManifest.UniqueID });
    }
}