using System.Reactive.Subjects;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace CompanionAdventures.Framework.Models;

/// <summary>
/// Class responsible for holding and managing the state of a single farmer leader
///
/// Uses reactive to signal subscribes of events/updates
/// </summary>
/// <param name="npc"></param>
public class Leader: IDisposable
{
    private readonly Store store;
    
    public Farmer Farmer;
    public List<Companion> Companions = new List<Companion>();
    
    public readonly BehaviorSubject<Vector2> Tile;
    public readonly BehaviorSubject<GameLocation> Location;
    
    
    public Leader(Store store, Farmer farmer)
    {
        this.store = store;
        this.Farmer = farmer;
        
        IMonitor monitor = store.UseMonitor();
        monitor.Log($"Creating Leader instance for {farmer.Name}");
        
        Tile = new BehaviorSubject<Vector2>(farmer.Tile);
        Location = new BehaviorSubject<GameLocation>(farmer.currentLocation);
    }

    public void AddCompanion(NPC npc)
    {
        ModConfig config = store.UseConfig();
        IMonitor monitor = store.UseMonitor();
        
        // Early Exit: If farmer has more than or equal to maximum number of companions
        if (Companions.Count >= config.MaxCompanions)
        {
            monitor.Log($"Could not add {npc.Name} as a companion to {Farmer.Name}. {Farmer.Name} already has the maximum number of companions!", LogLevel.Trace);
            return;
        }
        
        Companion companion = new Companion(store, npc, this);
        Companions.Add(companion);
        
        monitor.Log($"Successfully added {companion.npc.Name} as a companion to {Farmer.Name}.", LogLevel.Trace);
    }
    
    public void RemoveCompanion(NPC npc)
    {
        IMonitor monitor = store.UseMonitor();
        
        Companion? companion = Companions.Find(companion => companion.npc == npc);
        
        if (companion == null)
        {
            monitor.Log($"Could not remove {npc.Name} as a companion. {npc.Name} is not a companion of {Farmer.Name}!", LogLevel.Trace);
            return;
        }
        
        Companions.Remove(companion);
        companion.Dispose();
    }

    public bool IsCompanion(NPC npc)
    {
        return Companions.Any(companion => companion.npc == npc);
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
    
    public void Dispose()
    {
        IMonitor monitor = store.UseMonitor();
        monitor.Log($"Removing Leader instance for {Farmer.Name}");
        
        foreach (Companion companion in Companions)
        {
            companion.Dispose();
        }
    }
}