using CompanionAdventures.Framework.Models;

namespace CompanionAdventures.Framework;

using StardewModdingAPI;
using StardewValley;

public partial class Store
{
    private Companions? _companions = null;
    
    /// <summary>
    /// Sets the internal Companions instance if Companions is currently null. If an instance already
    /// exists does nothing.
    /// </summary>
    /// <param name="companions"></param>
    public void _Companions(Companions companions)
    {
        _companions ??= companions;
    }
    
    public Companions UseCompanions()
    {
        if (_companions == null)
            Companions.CreateStore(this);
        
        return _companions!;
    }
}

public class Companions
{
    /****
     ** Store Setup
     ****/
    private Store store;
    private Companions(Store store)
    {
        this.store = store;
    }
    public static void CreateStore(Store store)
    {
        store._Companions(new Companions(store));
    }
    
    /****
     ** Companions
     ****/
    public Dictionary<Farmer, List<NPC>> CurrentCompanions = new();
    
    public int CompanionHeartsThreshold = 0;
    public List<string> ValidCompanions = new List<string> {"Abigail", "Penny"};
    
    // Functions this needs to handle
    // Add companion local/net
    //  Remove from default scheduling
    // Remove companion local/net
    //  Resume default scheduling
    // Update companion location local/net
    // Handle game tick

    public void DrawCompanions(Farmer farmer)
    {
        foreach (var entry in CurrentCompanions)
        {
            // If the selected row is not our current farmer skip it
            if (entry.Key != farmer)
            {
                continue;
            }

            foreach (NPC npc in entry.Value)
            {
                npc.position.X = (int)farmer.position.X;
                npc.position.Y = (int)farmer.position.Y;
            }
        }
        
        // For NPC in CompanionManager.CurrentCompanions
        // check if NPC farmer is current farmer
        // update NPC location to follow current farmer
    }
    
    /// <summary>
    /// Attempts to add the npc to the farmers companion list
    /// </summary>
    /// <returns>
    /// Returns true if the companion was added or false if it couldn't be added.
    /// </returns>
    public bool AddCompanion(Farmer farmer, NPC npc)
    {
        ModConfig config = store.UseConfig();
        IMonitor monitor = store.UseMonitor();
        
        monitor.Log($"Attempting to add {npc.Name} as a companion to {farmer.Name}.", LogLevel.Trace);
        
        // Check if NPC is already a companion
        if (IsCompanion(npc))
        {
            monitor.Log($"Could not add {npc.Name} as a companion to {farmer.Name}. {npc.Name} is already a companion!", LogLevel.Trace);
            return false;
        }
        
        // Get farmers list of companions if they have one
        if (CurrentCompanions.TryGetValue(farmer, out List<NPC>? companions))
        {
            // If farmer has more than or equal to the maximum number of companions
            if (companions.Count >= config.MaxCompanions)
            {
                monitor.Log(
                    $"Could not add {npc.Name} as a companion to {farmer.Name}. {farmer.Name} already has the maximum number of companions!",
                    LogLevel.Trace
                );
                return false;
            }

            // If farmer doesn't have the ore than or equal to the maximum number of companions, add this companion
            companions.Add(npc);
            monitor.Log($"Successfully added {npc.Name} as a companion to {farmer.Name}.", LogLevel.Trace);
            CompanionData data = new CompanionData(farmer, npc);
            
            IModHelper helper = store.UseHelper();
            IManifest modManifest = store.UseManifest();
            
            helper.Multiplayer.SendMessage(data, Constants.MessagetypeCompanionAdded, new []{ modManifest.UniqueID });
            return true;
        }
        // Farmer doesn't exist in dictionary, add them and create a new list
        // Because farmer doesn't exist this means that they must have 0 companions, and it should be safe to add this
        // companion
        else
        {
            CurrentCompanions.Add(farmer, new List<NPC> { npc });
            monitor.Log($"Successfully added {npc.Name} as a companion to {farmer.Name}.", LogLevel.Trace);
            return true;
        }
    }

    public bool IsNpcValidCompanion(NPC npc)
    {
        IMonitor monitor = store.UseMonitor();
        
        monitor.Log($"Checking if {npc.Name} can be a valid companion.", LogLevel.Trace);

        if (ValidCompanions.Contains(npc.Name))
        {
            monitor.Log($"{npc.Name} can be a companion.", LogLevel.Trace);
            return true;
        }
        
        monitor.Log($"{npc.Name} can not be a companion.", LogLevel.Trace);
        return false;
    }

    public bool IsNpcValidCompanionForFarmer(Farmer farmer, NPC npc)
    {
        IMonitor monitor = store.UseMonitor();
        
        monitor.Log($"Checking if {npc.Name} can be a valid companion for {farmer.Name}.", LogLevel.Trace);
        // Early Exit: If NPC can't be a companion return
        if (!IsNpcValidCompanion(npc))
            return false;
        
        // Get the heart level of the farmer and this npc
        var hearts = Util.GetHeartLevel(farmer, npc);

        // Return true if number of hearts is equal to or above heart threshold
        if (hearts >= CompanionHeartsThreshold)
        {
            monitor.Log($"{npc.Name} can be a valid companion for {farmer.Name}.", LogLevel.Trace);
            return true;
        }
        
        monitor.Log($"{npc.Name} is not a valid companion for {farmer.Name}.", LogLevel.Trace);
        return false;
    }
    
    public List<NPC>? GetCurrentCompanions(Farmer player)
    {
        return CurrentCompanions.GetValueOrDefault(player);
    }

    public bool IsCompanion(NPC npc)
    {
        foreach (var entry in CurrentCompanions)
        {
            if (entry.Value.Contains(npc))
                return true;
        }

        return false;
    }

    public bool IsCurrentlyCompanionForFarmer(Farmer farmer, NPC npc)
    {
        // Try to get companions for this farmer
        if (CurrentCompanions.TryGetValue(farmer, out List<NPC>? companions))
        {
            // Check if list of companions contains NPC
            return companions.Contains(npc);
        }

        // Farmer does not have any companions so this npc isn't currently a companion
        return false;
    }

    public void CompanionAdd(Farmer farmer, NPC npc)
    {
        IMonitor monitor = store.UseMonitor();
        
        // Create dialog options
        string dialogText = $"Ask {npc.Name} to follow?";
        Response[] responses =
        [
            new Response("yes_key", "Yes"),
            new Response("no_key", "No"),
        ];
                
        void AfterQuestionBehaviour(Farmer farmer, string responseText)
        {
            if (responseText == "yes_key")
            {
                AddCompanion(farmer, npc);
                npc.controller = null;
                npc.temporaryController = null;
                monitor.Log($"Is {npc.Name} a companion? {IsCompanion(npc)}");
            }
            else
            {
                        
            }
        }

        Game1.currentLocation.createQuestionDialogue(dialogText, responses, AfterQuestionBehaviour, npc);
    }

    public void CompanionOptions(Farmer farmer, NPC npc)
    {
        IMonitor monitor = store.UseMonitor();
        
        string dialogText = $"Ask {npc.Name} to leave?";
        Response[] responses =
        [
            new Response("yes_key", "Yes"),
            new Response("no_key", "No"),
        ];
                    
        void AfterQuestionBehaviour(Farmer farmer, string responseText)
        {
            if (responseText == "yes_key")
            {
                monitor.Log($"Is {npc.Name} a companion? {IsCompanion(npc)}");
            }
            else
            {
                        
            }
        }

        Game1.currentLocation.createQuestionDialogue(dialogText, responses, AfterQuestionBehaviour, npc);
    }

    public void CompanionUpdate()
    {
        
    }

    /****
     ** Events
     ****/
    public void OnCompanionAdded(CompanionData data)
    {
        
    }

    public void OnCompanionRemoved(CompanionData data)
    {
        
    }

    /// <summary>
    /// Uses the provided data to update the provided companion with the provided information
    ///
    /// Useful when the companion is managed by another source (eg: another player in a multiplayer game) and the
    /// position or state information has been already calculated by another client and the display just needs to be
    /// updated in this client.
    /// </summary>
    /// <param name="data"></param>
    public void OnCompanionUpdated(CompanionData data)
    {
        
    }

    public void OnPlayerWarped(Farmer farmer, GameLocation newLocation)
    {
        IMonitor monitor = store.UseMonitor();
        monitor.Log($"Farmer is currently at {farmer.currentLocation}");
        monitor.Log($"Farmer is warping to {newLocation}");
        
        foreach (var entry in CurrentCompanions)
        {
            // If the selected row is not our current farmer skip it
            if (entry.Key != farmer)
            {
                continue;
            }

            foreach (NPC npc in entry.Value)
            {
                monitor.Log($"Updating companion {npc.Name}'s location to {newLocation}");
                Game1.warpCharacter(npc, newLocation, farmer.Tile);
                npc.currentLocation = newLocation;
            }
        }
        
        // Update current farmers NPCs so that they warp to the same map as farmer
    }

    public void OnUpdateTicking()
    {
        // Draw all NPCs
    }
}