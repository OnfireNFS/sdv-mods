// using System.Reactive.Subjects;
// using Force.DeepCloner;
// using Microsoft.Xna.Framework;
// using StardewModdingAPI;
// using StardewModdingAPI.Events;
// using StardewValley;
// using StardewValley.Locations;
// using StardewValley.Pathfinding;
//
// namespace CompanionAdventures.Framework.Models;
//
// /// <summary>
// /// Class responsible for holding and managing the state of a companion that is returning from an adventure
// /// </summary>
// public class ReturningCompanion
// {
//     private readonly Store store;
//     public NPC npc;
//     
//     private Dictionary<int, SchedulePathDescription>? _npcSchedule;
//     private SchedulePathDescription _currentSchedule;
//     private bool _warp;
//     
//     private ReturningCompanion(
//         Store store, 
//         NPC npc, 
//         SchedulePathDescription scheduleToFollow,
//         bool warp = false
//     )
//     {
//         this.store = store;
//         this.npc = npc;
//         // Backup the npcs regular schedule by cloning it
//         _npcSchedule = npc.Schedule.DeepClone();
//         _currentSchedule = scheduleToFollow;
//         _warp = warp;
//         
//         store.Monitor.Log($"Creating ReturningCompanion instance for {npc.Name}");
//
//         LoadNewSchedule(scheduleToFollow);
//         RegisterEvents();
//     }
//
//     private void LoadNewSchedule(SchedulePathDescription newSchedule)
//     {
//         // Stop current pathing
//         npc.controller = null;
//         npc.temporaryController = null;
//         npc.isMovingOnPathFindPath.Value = false;
//         
//         // Remove any other entries from the NPCs schedule
//         npc.ClearSchedule();
//         // Add the newly created return schedule to the npc's daily schedule
//         npc.TryLoadSchedule(Game1.timeOfDay.ToString(), new Dictionary<int, SchedulePathDescription>(){{ Game1.timeOfDay, newSchedule}});
//         
//         // The last attempted schedule could've been the same tick as the current time of day
//         // Set the last attempted check time to be 10 minutes before that so that the newly added
//         // schedule will always be run immediately
//         npc.lastAttemptedSchedule = Game1.timeOfDay - 10;
//         
//         // Run check schedule to run the return to location schedule
//         npc.checkSchedule(Game1.timeOfDay);
//     }
//     
//     /// <summary>
//     /// Checks if the provided NPC can find a path from their current location to the location they should be at
//     /// according to their schedule, if not it instead checks if the current NPC can find a warp to leave their current
//     /// location, if that's not possible either then just warp the NPC out of the location they are at.
//     /// </summary>
//     /// <returns>
//     /// Returns a ReturningCompanion instance if the NPC is walking back or walking to a warp. If the NPC was
//     /// immediately warped then returns `null`.
//     /// </returns>
//     public static ReturningCompanion? CreateReturningCompanionOrWarp(Store store, NPC npc)
//     {
//         // Try to load the npc's schedule for today
//         npc.ignoreScheduleToday = false;
//         npc.TryLoadSchedule();
//
//         // Try to get previous schedule this npc should've followed if they weren't a companion
//         SchedulePathDescription? previousSchedule = GetPreviousSchedule(npc);
//         
//         // Generate a new route from the npcs current location to wherever they should currently be according to their
//         // schedule or default location
//         SchedulePathDescription returnToSchedule = npc.pathfindToNextScheduleLocation(
//             npc.ScheduleKey, // This is only used to show error messages
//             npc.currentLocation.Name,
//             (int) npc.Tile.X,
//             (int) npc.Tile.Y,
//             previousSchedule?.targetLocationName ?? npc.DefaultMap,
//             previousSchedule?.targetTile.X ?? (int) npc.DefaultPosition.X / 64,
//             previousSchedule?.targetTile.Y ?? (int) npc.DefaultPosition.Y / 64,
//             previousSchedule?.facingDirection ?? npc.DefaultFacingDirection,
//             previousSchedule?.endOfRouteBehavior ?? null,
//             previousSchedule?.endOfRouteMessage ?? null
//         );
//         
//         // Were we able to make a valid path from the current location to where the npc should be?
//         // If the destination location is the Farm or FarmHouse then consider this route invalid (Pathing gets weird 
//         // around the farm)
//         if (returnToSchedule.route is { Count: > 0 } 
//             && !new List<string>{"Farm", "FarmHouse"}.Contains(returnToSchedule.targetLocationName)
//         )
//         {
//             store.Monitor.Log($"Created route for {npc.Name} from {npc.currentLocation.Name} to {returnToSchedule.targetLocationName}");
//             
//             return new ReturningCompanion(store, npc, returnToSchedule);
//         }
//         
//         store.Monitor.Log($"Could not find route for {npc.Name} from {npc.currentLocation.Name} to {returnToSchedule.targetLocationName}");
//         // Attempt to find an exit from this location
//         Warp? warp = npc.currentLocation.warps.FirstOrDefault<Warp>();
//         
//         // We couldn't find a path out of this location, just Warp to next location (¯\_(ツ)_/¯ we tried)
//         if (warp == null)
//         {
//             store.Monitor.Log($"Could not find exit from {npc.currentLocation.Name} for {npc.Name}. Warping...");
//             Game1.warpCharacter(
//                 npc, 
//                 previousSchedule?.targetLocationName ?? npc.DefaultMap,
//                 previousSchedule?.targetTile ?? new Point((int)npc.DefaultPosition.X / 64, (int)npc.DefaultPosition.Y / 64)
//             );
//
//             // Return this NPC to whatever it was doing before it was a companion
//             ResetNpc(store, npc, previousSchedule);
//             return null;
//         }
//         
//         // We found a warp we can path to, attempt to leave this location through that warp
//         SchedulePathDescription returnToWarp = npc.pathfindToNextScheduleLocation(
//             npc.ScheduleKey, // This is only used to show error messages
//             npc.currentLocation.Name,
//             (int) npc.Tile.X,
//             (int) npc.Tile.Y,
//             npc.currentLocation.Name,
//             warp.X,
//             warp.Y,
//             npc.DefaultFacingDirection,
//             previousSchedule?.endOfRouteBehavior ?? null,
//             previousSchedule?.endOfRouteMessage ?? null
//         );
//         
//         // TODO: Future improvement? Custom routing to leave farmhouse for immersion?
//         
//         // If a valid path to the warp was found create a new ReturningCompanion to route to it
//         if (returnToWarp.route is { Count: > 0 }
//             && !new List<string>{"Farm", "FarmHouse"}.Contains(returnToWarp.targetLocationName)
//         ) 
//             return new ReturningCompanion(store, npc, returnToWarp, true);
//         
//         // If a valid path to the warp could not be created warp to default location
//         store.Monitor.Log($"Could not create path to exit from {npc.currentLocation.Name} for {npc.Name}. Warping...");
//         Game1.warpCharacter(
//             npc, 
//             previousSchedule?.targetLocationName ?? npc.DefaultMap,
//             previousSchedule?.targetTile ?? new Point((int)npc.DefaultPosition.X / 64, (int)npc.DefaultPosition.Y / 64)
//         );
//
//         // Return this NPC to whatever it was doing before it was a companion
//         ResetNpc(store, npc, previousSchedule);
//         return null;
//     }
//     
//     private static SchedulePathDescription? GetPreviousSchedule(NPC npc, Dictionary<int, SchedulePathDescription>? schedule = null)
//     {
//         schedule ??= npc.Schedule;
//         
//         // Early Exit: If npc Schedule is null than there can't be a previous schedule, return null
//         if (schedule == null || schedule.Count == 0)
//             return null;
//         
//         // If the npc's schedule isn't empty, use the current time to check if one of the entries should've happened
//         // already
//         SchedulePathDescription? previousSchedule = null;
//         int currentTime = Game1.timeOfDay;
//         
//         // Start at the current time of day and work back to the last scheduled path
//         // Check all the way to 0 instead of stopping at 600 to see if a zero schedule exists
//         while (currentTime > 0)
//         {
//             if (schedule!.TryGetValue(currentTime, out SchedulePathDescription? _schedule))
//             {
//                 previousSchedule = _schedule;
//                 break;
//             }
//         
//             currentTime -= 10;
//         }
//         
//         return previousSchedule;
//     }
//
//     private static void ResetNpc(Store store, NPC npc, SchedulePathDescription? previousSchedule = null)
//     {
//         // Load the default npc schedule
//         npc.ClearSchedule();
//         npc.TryLoadSchedule();
//         
//         // Run check schedule to see if the NPC should be doing something right now
//         npc.checkSchedule(Game1.timeOfDay);
//         
//         // Early Exit: If previous schedule was null (nothing to do) or NPC already has a controller exit (NPC already
//         // doing something) do nothing
//         if ( previousSchedule == null || npc.controller != null) 
//             return;
//         
//         store.Monitor.Log("NPC Controller was null and previous schedule was not null, attempting to change direction and end behavior");
//         npc.controller =
//             new PathFindController(npc, npc.currentLocation, npc.TilePoint, previousSchedule.facingDirection);
//         npc.StartActivityRouteEndBehavior(previousSchedule.endOfRouteBehavior, previousSchedule.endOfRouteMessage);
//     }
//     
//     /****
//      ** Events
//      ****/
//     private void RegisterEvents()
//     {
//         store.Monitor.Log($"Registering events for ReturningCompanion {npc.Name}");
//         store.Helper.Events.GameLoop.OneSecondUpdateTicking += OnOneSecondUpdateTicking;
//
//         // If this companion isn't warping out of this location, add an event handler for updating their path when the
//         // time changes, this can be useful if they were originally supposed to path to their 900 location when they
//         // left but walking took so long they should now be pathing to their 1000 location
//         if (!_warp)
//         {
//             store.Helper.Events.GameLoop.TimeChanged += OnTimeChanged;
//         }
//     }
//     
//     private void UnregisterEvents()
//     {
//         store.Monitor.Log($"Unregistering events for ReturningCompanion {npc.Name}");
//         store.Helper.Events.GameLoop.TimeChanged -= OnTimeChanged;
//         store.Helper.Events.GameLoop.OneSecondUpdateTicking -= OnOneSecondUpdateTicking;
//     }
//
//     private void OnOneSecondUpdateTicking(object? sender, OneSecondUpdateTickingEventArgs e)
//     {
//         // If this companion is not set to be warped, and its current tile X & Y equal the target tile X & Y
//         // Then it has reached its destination, remove this ReturningCompanion instance
//         if (!_warp && (int) npc.Tile.X == _currentSchedule.targetTile.X && (int) npc.Tile.Y == _currentSchedule.targetTile.Y)
//         {
//             store.Monitor.Log($"{npc.Name} has made it to destination tile!");
//             store.Companions.RemoveReturningCompanion(npc);
//         }
//
//         if (_warp)
//         {
//             // Early Exit: If NPC is currently on the map they started on then they haven't reached the exit of their 
//             // current map, and we shouldn't warp them yet.
//             if (_currentSchedule.targetLocationName == npc.currentLocation.Name)
//                 return;
//         
//             store.Monitor.Log($"{npc.Name} has made it to destination warp!");
//             store.Monitor.Log($"Warping {npc.Name} to correct location and tile");
//             
//             SchedulePathDescription? previousSchedule = null;
//             if (_npcSchedule != null)
//                 previousSchedule = GetPreviousSchedule(npc, _npcSchedule);
//             
//             Console.WriteLine($"{previousSchedule?.targetLocationName ?? npc.DefaultMap}");
//             Console.WriteLine($"{previousSchedule?.targetTile ?? new Point((int)npc.DefaultPosition.X / 64, (int)npc.DefaultPosition.Y / 64)}");
//         
//             Game1.warpCharacter(
//                 npc, 
//                 previousSchedule?.targetLocationName ?? npc.DefaultMap,
//                 previousSchedule?.targetTile ?? new Point((int)npc.DefaultPosition.X / 64, (int)npc.DefaultPosition.Y / 64)
//             );
//         
//             store.Companions.RemoveReturningCompanion(npc);
//         }
//     }
//     
//     private void OnTimeChanged(object? sender, TimeChangedEventArgs e)
//     {
//         // Early Exit: If the stored npc schedule is null, or we couldn't get a new schedule for the current time,
//         // then do nothing
//         if (_npcSchedule == null || !_npcSchedule.TryGetValue(e.NewTime, out SchedulePathDescription? schedule)) 
//             return;
//         
//         store.Monitor.Log($"New schedule for {npc.Name} found at {e.NewTime}");
//         // Generate a new route from the npcs current location to wherever they should currently be according to
//         // the new schedule
//         SchedulePathDescription returnToSchedule = npc.pathfindToNextScheduleLocation(
//             npc.ScheduleKey, // This is only used to show error messages
//             npc.currentLocation.Name,
//             (int) npc.Tile.X,
//             (int) npc.Tile.Y,
//             schedule.targetLocationName,
//             schedule.targetTile.X,
//             schedule.targetTile.Y,
//             schedule.facingDirection,
//             schedule.endOfRouteBehavior,
//             schedule.endOfRouteMessage
//         );
//
//         // Early Exit: If the newly created schedules route is null, do nothing.
//         if (returnToSchedule.route == null) 
//             return;
//         
//         store.Monitor.Log($"Updating {npc.Name}'s return path to end at {e.NewTime} schedule destination");
//         // Update the schedule the npc uses to determine if it has reached its destination to be this schedule
//         _currentSchedule = returnToSchedule;
//         
//         LoadNewSchedule(returnToSchedule);
//     }
//
//     public void Remove()
//     {
//         store.Monitor.Log($"Removing ReturningCompanion instance for {npc.Name}");
//         
//         UnregisterEvents();
//         
//         // Load the default npc schedule
//         npc.TryLoadSchedule();
//
//         SchedulePathDescription? previousSchedule = GetPreviousSchedule(npc);
//         ResetNpc(store, npc, previousSchedule);
//     }
// }