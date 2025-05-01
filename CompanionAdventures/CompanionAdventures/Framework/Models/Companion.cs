using Microsoft.Xna.Framework;
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
    public Leader leader;
    private IDisposable leaderLocation;

    public Companion(Store store, NPC npc, Leader leader)
    {
        this.store = store;
        this.npc = npc;
        this.leader = leader;

        this.leaderLocation = leader.Tile.Subscribe(
            tile => UpdateTile(tile)
        );
    }

    private void UpdateTile(Vector2 tile)
    {
        IMonitor monitor = store.UseMonitor();
        
        npc.position.X = (int)tile.X * 64;
        npc.position.Y = (int)tile.Y * 64;
        
        monitor.Log($"{npc.Name}: Updating position to {tile.X * 64}, {tile.Y * 64}");
    }

    public void UpdateLocation(GameLocation newLocation)
    {
        IMonitor monitor = store.UseMonitor();
        
        monitor.Log($"Updating companion {npc.Name}'s location to {newLocation}");
        Game1.warpCharacter(npc, newLocation, leader.Farmer.Tile);
    }

    public void Remove()
    {
        leaderLocation.Dispose();
    }
}