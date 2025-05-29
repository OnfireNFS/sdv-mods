using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Pathfinding;

namespace CompanionAdventures.Framework.Models;

/// <summary>
/// Class responsible for holding and managing the state of a single companion
/// </summary>
public class CompanionOld
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
        
        store.Monitor.Log($"Creating Companion instance for {npc.Name}");

        this.leaderTile = leader.Tile.Subscribe(UpdateTile);
        this.leaderLocation = leader.Location.Subscribe(UpdateLocation);
        
        // Stop NPC Movement
        npc.controller = null;
        npc.temporaryController = null;
        npc.isMovingOnPathFindPath.Value = false;
        
        // Clear NPCs schedule
        npc.ClearSchedule();
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
        
        store.Monitor.Log($"Updating companion {npc.Name}'s location to {newLocation}");
        
        Game1.warpCharacter(npc, newLocation, leader.Tile.Value);
    }

    /****
     ** Events
     ****/
    private void RegisterEvents()
    {
        store.Monitor.Log($"Registering events for Companion {npc.Name}");
        store.Helper.Events.GameLoop.UpdateTicking += OnUpdateTicking;
    }

    private void UnregisterEvents()
    {
        store.Monitor.Log($"Unregistering events for Companion {npc.Name}");
        store.Helper.Events.GameLoop.UpdateTicking -= OnUpdateTicking;
    }
    
    private void OnUpdateTicking(object? sender, UpdateTickingEventArgs e)
    {
        // Calculate new position for this npc or something?
    }
    
    public void Remove()
    {
        store.Monitor.Log($"Removing Companion instance for {npc.Name}");
        
        // Remove reactive listeners
        leaderTile.Dispose();
        leaderLocation.Dispose();
    }
}