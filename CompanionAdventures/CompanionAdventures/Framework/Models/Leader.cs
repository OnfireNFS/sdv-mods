using System.Reactive.Subjects;
using Microsoft.Xna.Framework;
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
    public readonly BehaviorSubject<Vector2> Tile = null!;
    public readonly BehaviorSubject<GameLocation> Location = null!;
    
    
    public Leader(Farmer farmer)
    {
        this.Farmer = farmer;
        Tile = new BehaviorSubject<Vector2>(farmer.Tile);
        Location = new BehaviorSubject<GameLocation>(farmer.currentLocation);
    }

    public void UpdateTile(Vector2 tile)
    {
        // Early exit: If stored matches current tile do nothing
        if (tile.Equals(Tile.Value))
            return;
        
        Tile.OnNext(tile);
    }

    public void UpdateTile(GameLocation location, Vector2 tile)
    {
        // Early exit: if the location is the same as the previous one do nothing
        if (location.Equals(Location.Value))
            return;
        
        Location.OnNext(location);
        Tile.OnNext(tile);
    }
}