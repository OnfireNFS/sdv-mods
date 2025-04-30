using StardewModdingAPI;
using StardewModdingAPI.Events;

#nullable disable
namespace CompanionAdventures.Framework;

public class MultiplayerManager
{
    private static MultiplayerManager Instance;
    
    private readonly IManifest ModManifest;
    private readonly IMonitor Monitor;
    private readonly IMultiplayerHelper Multiplayer;

    private MultiplayerManager(CompanionAdventures mod, IModHelper helper)
    {
        ModManifest = mod.ModManifest;
        Monitor = mod.Monitor;
        Multiplayer = helper.Multiplayer;
    }
    
    public static MultiplayerManager New(CompanionAdventures mod, IModHelper helper)
    {
        Instance ??= new MultiplayerManager(mod, helper);

        return Instance;
    }
    
    public void SendMessage(string message)
    {
        string data = "Test Data";
        Multiplayer.SendMessage(data, "companionadventures.companion.add", new []{ ModManifest.UniqueID });
    }

    public void OnMessageReceived(object sender, ModMessageReceivedEventArgs e)
    {
        // Early Exit: The message received was from a different mod
        if (e.FromModID != ModManifest.UniqueID)
        {
            return;
        }

        string data = e.ReadAs<string>();
        Monitor.Log($"Received \"{e.Type}\" event with data: {data}", LogLevel.Trace);
        // if (e.Type == "companionadventures.companion.add")
        // {
        //     
        // }
    }
}