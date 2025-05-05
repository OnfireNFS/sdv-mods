using System.Reactive.Subjects;
using Force.DeepCloner;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Pathfinding;

namespace CompanionAdventures.Framework.Models;

/// <summary>
/// Class responsible for holding and managing the state of a companion that is returning from an adventure
/// </summary>
public class ReturningCompanion
{
    private readonly Store store;
    public NPC npc;
    
    private Dictionary<int, SchedulePathDescription> _npcSchedule = null!;
    private SchedulePathDescription? _currentScheduleEntry;
    private Point _defaultTile;
    private string _defaultLocation = null!;
    
    public ReturningCompanion(Store store, NPC npc)
    {
        this.store = store;
        this.npc = npc;
        
        IMonitor monitor = store.UseMonitor();
        monitor.Log($"Creating ReturningCompanion instance for {npc.Name}");
        
        CreateRoute();
    }
    
    /// <summary>
    /// Stop returning route and unregister this NPC as being a ReturningCompanion
    /// 
    /// (Useful when adding a NPC as a companion but that NPC is currently returning)
    /// Example: Player 1 had Abigail as companion, Player 1 dismisses Abigail, Abigail becomes a ReturningCompanion,
    /// before Abigail reaches her destination Player 2 asks Abigail to be a companion.
    /// </summary>
    public void Cancel()
    {
        IMonitor monitor = store.UseMonitor();
        monitor.Log($"Cancelling ReturningCompanion instance for {npc.Name}");
        
        Companions companions = store.UseCompanions();
        companions.RemoveReturningCompanion(npc);
    }

    private void Complete()
    {
        IMonitor monitor = store.UseMonitor();
        monitor.Log($"Completed ReturningCompanion instance for {npc.Name}");
        
        Companions companions = store.UseCompanions();
        companions.RemoveReturningCompanion(npc);
    }
    
    private void CreateRoute()
    {
        IMonitor monitor = store.UseMonitor();
        
        // Try to load the npc's schedule for today
        npc.ignoreScheduleToday = false;
        npc.TryLoadSchedule();

        // Clone the default npc schedule, do a deep clone so all the schedule references are cloned as well
        _npcSchedule = npc.Schedule.DeepClone();
        
        // If the npc's schedule isn't empty, use the current time to check if one of the entries should've happened
        // already
        SchedulePathDescription? previousSchedule = null;
        if (_npcSchedule?.Count > 0)
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

        Point marriedTile = Point.Zero;
        string? marriedLocation = null;
        if (npc.isMarried())
        {
            Farmer? spouse = npc.getSpouse();

            if (spouse != null)
            {
                GameLocation? farm = Game1.getLocationFromName(spouse.homeLocation.Value);
                
                if (farm is FarmHouse farmHouse)
                {
                    marriedTile = farmHouse.getSpouseBedSpot(npc.Name);
                    marriedLocation = spouse.homeLocation.Value;
                }
            }
        }

        // setup default locations, depends on if npc is married or not
        // TODO: The default marriage location should be the players house, not this spot on at the BusStop
        _defaultTile = marriedTile.Equals(Point.Zero)
            ? new Point((int)npc.DefaultPosition.X / 64, (int)npc.DefaultPosition.Y / 64)
            : marriedTile;
        _defaultLocation = marriedLocation ?? npc.DefaultMap;
        
        // Generate a new route from the npc's current location to wherever they should currently be according to their
        // schedule
        SchedulePathDescription returnToSchedule = npc.pathfindToNextScheduleLocation(
            npc.ScheduleKey, // This is only used to show error messages
            npc.currentLocation.Name,
            (int) npc.Tile.X,
            (int) npc.Tile.Y,
            previousSchedule?.targetLocationName ?? _defaultLocation,
            previousSchedule?.targetTile.X ?? _defaultTile.X,
            previousSchedule?.targetTile.Y ?? _defaultTile.Y,
            previousSchedule?.facingDirection ?? npc.DefaultFacingDirection,
            previousSchedule?.endOfRouteBehavior ?? null,
            previousSchedule?.endOfRouteMessage ?? null
        );

        // Were we able to make a valid path from the current location to where the npc should be?
        // if (returnToSchedule.route != null)
        if (false)
        {
            monitor.Log($"Created route for {npc.Name} from {npc.currentLocation.Name} to {returnToSchedule.targetLocationName}");
            // Save a copy of the current return schedule (we'll use this later to see if we need to update the
            // schedule)
            _currentScheduleEntry = returnToSchedule;
            // We don't need to clear the schedule here, but it reduces the likeliness of other entries interfering
            // with the return path schedule
            npc.Schedule.Clear();
            // Add the newly created return schedule to the npc's daily schedule
            npc.Schedule?.Add(Game1.timeOfDay, returnToSchedule);
        }
        else
        {
            monitor.Log($"Could not find route for {npc.Name} from {npc.currentLocation.Name} to {returnToSchedule.targetLocationName}");
            // Attempt to find an exit from this location
            Warp? warp = npc.currentLocation.warps.FirstOrDefault<Warp>();
            
            // We couldn't find a path out of this location, just Warp to next location (¯\_(ツ)_/¯ we tried)
            // if (warp == null)
            if (true)
            {
                monitor.Log($"Could not find exit from {npc.currentLocation.Name} for {npc.Name}. Warping...");
                Game1.warpCharacter(
                    npc, 
                    previousSchedule?.targetLocationName ?? _defaultLocation,
                    previousSchedule?.targetTile ?? _defaultTile
                );
            
                // TODO: This doesn't work because the instance can't destroy itself 
                Companions companions = store.UseCompanions();
                companions.RemoveReturningCompanion(npc);
                return;
            }
            
            // We found a warp we can path to, leave this location through that warp
            SchedulePathDescription returnToWarp = npc.pathfindToNextScheduleLocation(
                npc.ScheduleKey, // This is only used to show error messages
                npc.currentLocation.Name,
                (int) npc.Tile.X,
                (int) npc.Tile.Y,
                npc.currentLocation.Name,
                warp.X,
                warp.Y,
                npc.DefaultFacingDirection,
                previousSchedule?.endOfRouteBehavior ?? null,
                previousSchedule?.endOfRouteMessage ?? null
            );

            // Register event handler to check if npc has pathed to warp location
            IModHelper helper = store.UseHelper();
            helper.Events.GameLoop.OneSecondUpdateTicking += OnOneSecondUpdateTicking;
                
            // Save a copy of the current return schedule (we'll use this later to see if we need to update the
            // schedule)
            _currentScheduleEntry = returnToWarp;
            // We don't need to clear the schedule here, but it reduces the likeliness of other entries interfering
            // with the return path schedule
            npc.Schedule.Clear();
            // Add the newly created return schedule to the npc's daily schedule
            npc.Schedule?.Add(Game1.timeOfDay, returnToWarp);
        }
        
        // The last attempted schedule could've been the same tick as the current time of day
        // Set the last attempted check time to be 10 minutes before that so that the newly added
        // schedule will always be run immediately
        npc.lastAttemptedSchedule -= 10;
        
        // Run check schedule to run the return to location schedule
        npc.checkSchedule(Game1.timeOfDay);
        
        // Register events for this returning companion instance
        RegisterEvents();
        
        // TODO: What if returnToSchedule.route is null? A valid path could not be found between npc's current location
        //  and target location, maybe route towards exit and the warp them?
        //  Maybe write a check so that while the npc is on the "return" route we check to see if the previous 
        //  route is still where they should be pathing to. Eg: Abigail is dismissed on Monday at 8:40, she doesn't 
        //  have a previous schedule so she paths to her bed, by 9:10 she hasn't reached it, at this point she should
        //  be pathing to the kitchen, update route so that it is current location -> kitchen instead of current
        //  location -> bed
    }

    private void UpdateRoute()
    {
        // TODO: Get schedule
        //  check if new entry exists for time
        //  if entry doesn't exist -> return
        //  if entry does exist, update schedule then execute new path
    }
    
    private void RegisterEvents()
    {
        IModHelper helper = store.UseHelper();
        IMonitor monitor = store.UseMonitor();
        
        monitor.Log($"Registering events for ReturningCompanion {npc.Name}");
        helper.Events.GameLoop.TimeChanged += OnTimeChanged;
    }
    
    private void UnregisterEvents()
    {
        IModHelper helper = store.UseHelper();
        IMonitor monitor = store.UseMonitor();
        
        monitor.Log($"Unregistering events for ReturningCompanion {npc.Name}");
        helper.Events.GameLoop.TimeChanged -= OnTimeChanged;
        helper.Events.GameLoop.OneSecondUpdateTicking -= OnOneSecondUpdateTicking;
    }

    private void OnOneSecondUpdateTicking(object? sender, OneSecondUpdateTickingEventArgs e)
    {
        if (_currentScheduleEntry == null)
        {
            // Error: Something went wrong, this companion has registered the OnOneSecondUpdateTicking handler but
            // doesn't currently have a schedule. This means they will never reach their destination. Reset this npc.
            return;
        }

        // Early Exit: If NPC is currently on the map they started on then they haven't reached the exit of their 
        // current map, and we shouldn't warp them yet.
        if (_currentScheduleEntry.targetLocationName == npc.currentLocation.Name)
            return;
        
        IMonitor monitor = store.UseMonitor();
        monitor.Log($"{npc.Name} has made it to destination warp!");
        monitor.Log($"Warping {npc.Name} to correct location and tile");
            
        // Warp the NPC to the destination of the previous schedule according to the current time
        int currentTime = Game1.timeOfDay;
        SchedulePathDescription? previousSchedule = null;
            
        // Check all the way to 0 instead of stopping at 600 to see if a zero schedule exists
        while (currentTime > 0)
        {
            if (_npcSchedule.TryGetValue(currentTime, out SchedulePathDescription? _previousSchedule))
            { 
                monitor.Log($"Found previous schedule time: {currentTime}");
                previousSchedule = _previousSchedule;
                break;
            }
        
            currentTime -= 10;
        }
            
        Game1.warpCharacter(
            npc, 
            previousSchedule?.targetLocationName ?? _defaultLocation,
            previousSchedule?.targetTile ?? _defaultTile
        );
            
        Companions companions = store.UseCompanions();
        companions.RemoveReturningCompanion(npc);
    }
    
    private void OnTimeChanged(object? sender, TimeChangedEventArgs e)
    {
        // npc.Schedule.TryGetValue();
        // Check if the NPC that is currently returning should be pathing to a new location
        // If so update pathing
    }

    public void Remove()
    {
        IMonitor monitor = store.UseMonitor();
        monitor.Log($"Removing ReturningCompanion instance for {npc.Name}");
        
        UnregisterEvents();
        
        // Load the default npc schedule
        npc.TryLoadSchedule();
        
        // If we need to warp the npc we need to warp them to the starting location of the previous schedule,
        // not the target location. Which means we need to get the location before the previous schedule
        int currentTime = Game1.timeOfDay;
        SchedulePathDescription? previousSchedule = null;
                
        // Check all the way to 0 instead of stopping at 600 to see if a zero schedule exists
        while (currentTime > 0)
        {
            if (_npcSchedule.TryGetValue(currentTime, out SchedulePathDescription? _previousSchedule))
            { 
                monitor.Log($"Found previous schedule time: {currentTime}");
                previousSchedule = _previousSchedule;
                break;
            }
            
            currentTime -= 10;
        }
        
        // Run check schedule to run the return to location schedule
        npc.checkSchedule(Game1.timeOfDay);
        
        // Early Exit: If previous schedule was null (nothing to do) or NPC already has a controller exit (NPC already
        // doing something) do nothing
        if ( previousSchedule == null || npc.controller != null) 
            return;
        
        monitor.Log("NPC Controller was null and previous schedule was not null, attempting to change direction and end behavior");
        npc.controller =
            new PathFindController(npc, npc.currentLocation, npc.TilePoint, previousSchedule.facingDirection);
        npc.StartActivityRouteEndBehavior(previousSchedule.endOfRouteBehavior, previousSchedule.endOfRouteMessage);
    }
}