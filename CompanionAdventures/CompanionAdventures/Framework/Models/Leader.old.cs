using System.Reactive.Subjects;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace CompanionAdventures.Framework.Models;

/// <summary>
/// Class responsible for holding and managing the state of a single farmer leader
///
/// Uses reactive to signal subscribes of events/updates
/// </summary>
/// <param name="npc"></param>
public class Leader
{
    public Farmer Farmer;
    public List<Companion> Companions = new();
    
    public readonly BehaviorSubject<Vector2> Tile;
    public readonly BehaviorSubject<GameLocation> Location;
    
    public Leader(Farmer farmer)
    {
        this.Farmer = farmer;
        
        Tile = new BehaviorSubject<Vector2>(farmer.Tile);
        Location = new BehaviorSubject<GameLocation>(farmer.currentLocation);
    }
    public bool IsCompanion(NPC npc)
    {
        return Companions.Any(companion => companion.npc == npc);
    }
    
    /// <summary>
    /// Checks the Tile that the Leaders Farmer is currently at. If it is different from the Tile the Farmer was at the
    /// last time this function was called it will set Leader.Location (not Leader.Farmer.currentLocation) property
    /// which is reactive and will notify subscribers of the change. This function will also update Leader.Tile if
    /// the location changes. (Generally when a location changes the tile also changes because the player has warped to
    /// a different map)
    ///
    /// This function is cheap to run because it only does something when the data is actually changed
    /// </summary>
    public void UpdateLocation()
    {
        if(Location.Value.Equals(Farmer.currentLocation))
            return;
        
        Location.OnNext(Farmer.currentLocation);
        UpdateTile();
    }
    
    /// <summary>
    /// Checks the Location that the Leaders Farmer is currently at. If it is different from the Location the Farmer was
    /// at the last time this function was called it will set Leader.Location (not Leader.Farmer.Tile) property which is
    /// reactive and will notify subscribers of the change.
    ///
    /// This function is cheap to run because it only does something when the data is actually changed
    /// </summary>
    public void UpdateTile()
    {
        // Early exit: If the stored Tile matches current Farmer Tile do nothing
        if (Tile.Value.Equals(Farmer.Tile))
            return;
        
        Tile.OnNext(Farmer.Tile);
    }
    
    private void OnPlayerWarped(object? sender, WarpedEventArgs e)
    {
        if (e.Player != Farmer)
            return;
        
        // If the player that warped is our Farmer, run the update location function
        UpdateLocation();
    }
    
    private void OnUpdateTicking(object? sender, UpdateTickingEventArgs e)
    {
        // Every tick, check if our Farmers tile has changed
        UpdateTile();
    }
}