using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace CompanionAdventures.Framework.Models;

/// <summary>
/// Class responsible for holding and managing the state of a single companion
/// </summary>
/// <param name="npc"></param>
public class Companion
{
    private readonly Store store;
    public NPC npc;
    public Farmer farmer;

    public Companion(Store store, NPC npc, Farmer farmer)
    {
        this.store = store;
        this.npc = npc;
        this.farmer = farmer;
    }

    public void UpdateLocation(GameLocation newLocation)
    {
        IMonitor monitor = store.UseMonitor();
        
        monitor.Log($"Updating companion {npc.Name}'s location to {newLocation}");
        Game1.warpCharacter(npc, newLocation, farmer.Tile);
    }

    public void Remove()
    {
        
    }
}