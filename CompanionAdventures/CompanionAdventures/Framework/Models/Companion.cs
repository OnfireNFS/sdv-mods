using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Pathfinding;

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

        this.leaderTile = leader.Tile.Subscribe(UpdateTile);
        this.leaderLocation = leader.Location.Subscribe(UpdateLocation);
        
        // Stop NPC Movement
        npc.controller = null;
        npc.temporaryController = null;
        npc.isMovingOnPathFindPath.Value = false;
        
        // Clear NPCs schedule
        npc.ignoreScheduleToday = true;
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
        
        // Try to load the npc's schedule for today
        npc.ignoreScheduleToday = false;
        npc.TryLoadSchedule();
        
        // If the npc's schedule isn't empty, use the current time to check if one of the entries should've happened
        // already
        SchedulePathDescription? previousSchedule = null;
        if (npc.Schedule?.Count > 0)
        {
            int currentTime = Game1.timeOfDay;
            
            // Start at the current time of day and work back to the last scheduled path
            // Check all the way to 0 instead of stopping at 600 to see if a zero schedule exists
            while (currentTime > 0)
            {
                if (npc.Schedule.TryGetValue(currentTime, out SchedulePathDescription? schedule))
                {
                    previousSchedule = schedule;
                    break;
                }
            
                currentTime -= 10;
            }
        }

        // setup default locations, depends on if npc is married or not
        Point defaultTile = npc.isMarried()
            ? new Point(10, 23)
            : new Point((int)npc.DefaultPosition.X / 64, (int)npc.DefaultPosition.Y / 64);
        string defaultLocation = npc.isMarried() ? "BusStop" : npc.DefaultMap;
        
        // Generate a new route from the npc's current location to wherever they should currently be according to their
        // schedule
        SchedulePathDescription returnToSchedule = npc.pathfindToNextScheduleLocation(
            npc.ScheduleKey, // This is only used to show error messages
            npc.currentLocation.Name,
            (int) npc.Tile.X,
            (int) npc.Tile.Y,
            previousSchedule?.targetLocationName ?? defaultLocation,
            previousSchedule?.targetTile.X ?? defaultTile.X,
            previousSchedule?.targetTile.Y ?? defaultTile.Y,
            previousSchedule?.facingDirection ?? npc.DefaultFacingDirection,
            previousSchedule?.endOfRouteBehavior ?? null,
            previousSchedule?.endOfRouteMessage ?? null
        );

        // Were we able to make a valid path from the current location to where the npc should be?
        if (returnToSchedule.route != null)
        {
            // Add the newly created return schedule to the npc's daily schedule
            npc.Schedule?.Add(Game1.timeOfDay, returnToSchedule);
        }
        else
        {
            // TODO: Attempt to create a route to the exit of this map, then warp to 
        }
        
        // The last attempted schedule could've been the same tick as the current time of day
        // Set the last attempted check time to be 10 minutes before that so that the newly added
        // schedule will always be run immediately
        npc.lastAttemptedSchedule -= 10;
        
        // Run check schedule to run the return to location schedule
        npc.checkSchedule(Game1.timeOfDay);
        
        // TODO: What if returnToSchedule.route is null? A valid path could not be found between npc's current location
        //  and target location, maybe route towards exit and the warp them?
        //  Maybe write a check so that while the npc is on the "return" route we check to see if the previous 
        //  route is still where they should be pathing to. Eg: Abigail is dismissed on Monday at 8:40, she doesn't 
        //  have a previous schedule so she paths to her bed, by 9:10 she hasn't reached it, at this point she should
        //  be pathing to the kitchen, update route so that it is current location -> kitchen instead of current
        //  location -> bed
    }
}