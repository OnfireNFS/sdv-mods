using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace CompanionAdventures.Framework.Models;

/// <summary>
/// Class responsible for holding and managing the state of a single companion
/// </summary>
/// <param name="npc"></param>
public class Companion: IDisposable
{
    private readonly Store store;
    public NPC npc;
    public Leader leader;
    
    private IDisposable leaderTile;
    private IDisposable leaderLocation;

    public Companion(Store store, NPC npc, Leader leader)
    {
        this.store = store;
        this.npc = npc;
        this.leader = leader;
        
        IMonitor monitor = store.UseMonitor();
        monitor.Log($"Creating Companion instance for {npc.Name}");

        this.leaderTile = leader.Tile.Subscribe(
            tile => UpdateTile(tile)
        );
        this.leaderLocation = leader.Location.Subscribe(
            location => UpdateLocation(location)
        );
    }

    private void UpdateTile(Vector2 tile)
    {
        npc.position.X = (int)tile.X * 64;
        npc.position.Y = (int)tile.Y * 64;
    }

    public void UpdateLocation(GameLocation newLocation)
    {
        // Don't warp this Companion if they are already on this map
        if(npc.currentLocation.Equals(newLocation))
            return;
        
        IMonitor monitor = store.UseMonitor();
        monitor.Log($"Updating companion {npc.Name}'s location to {newLocation}");
        
        Game1.warpCharacter(npc, newLocation, leader.Tile.Value);
    }

    /****
     ** Events
     ****/
    private void RegisterEvents()
    {
        IModHelper helper = store.UseHelper();
        IMonitor monitor = store.UseMonitor();
        
        monitor.Log($"Registering events for Companion {npc.Name}");
        helper.Events.GameLoop.UpdateTicking += OnUpdateTicking;
    }

    private void UnregisterEvents()
    {
        IModHelper helper = store.UseHelper();
        IMonitor monitor = store.UseMonitor();
        
        monitor.Log($"Unregistering events for Companion {npc.Name}");
        helper.Events.GameLoop.UpdateTicking -= OnUpdateTicking;
    }
    
    private void OnUpdateTicking(object? sender, UpdateTickingEventArgs e)
    {
        // Calculate new position for this npc or something?
    }
    
    public void Dispose()
    {
        IMonitor monitor = store.UseMonitor();
        monitor.Log($"Removing Companion instance for {npc.Name}");
        
        leaderTile.Dispose();
        leaderLocation.Dispose();
    }
}