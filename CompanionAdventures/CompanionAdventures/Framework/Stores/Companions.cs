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

/// <summary>
/// Manages creating and removing companions for this player
/// </summary>
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
    public Dictionary<Farmer, List<Companion>> CurrentCompanions = new();
    
    public int CompanionHeartsThreshold = 0;
    public List<string> ValidCompanions = new List<string> {"Abigail", "Penny"};
    
    // Functions this needs to handle
    // Add companion local/net
    //  Remove from default scheduling
    // Remove companion local/net
    //  Resume default scheduling
    // Update companion location local/net
    // Handle game tick

    /// <summary>
    /// Attempts to add the npc as a companion to the provided farmer
    /// </summary>
    public void Add(Farmer farmer, NPC npc)
    {
        IMonitor monitor = store.UseMonitor();
        
        // Early Exit: Check if NPC is already a companion
        if (IsNpcCompanion(npc))
        {
            Companion existingCompanion = GetCompanion(npc)!;
            monitor.Log($"Could not add {npc.Name} as a companion to {farmer.Name}. {npc.Name} is already a companion for {existingCompanion.farmer.Name}!", LogLevel.Trace);
            return;
        }
        
        // Early Exit: Check if NPC can be a companion for this farmer
        if (!IsNpcValidCompanionForFarmer(farmer, npc))
        {
            monitor.Log($"Could not add {npc.Name} as a companion to {farmer.Name}. {npc.Name} is not a valid companion for {farmer.Name}!", LogLevel.Trace);
            return;
        }
        
        // Get farmers list of companions if they have one
        ModConfig config = store.UseConfig();
        List<Companion> currentCompanions = GetCurrentCompanions(farmer);

        // Early Exit: If farmer has more than or equal to maximum number of companions
        if (currentCompanions.Count >= config.MaxCompanions)
        {
            monitor.Log($"Could not add {npc.Name} as a companion to {farmer.Name}. {farmer.Name} already has the maximum number of companions!", LogLevel.Trace);
            return;
        }

        // Create new companion from NPC and add it to current companions for this farmer
        Companion companion = new Companion(store, npc, farmer);
        currentCompanions.Add(companion);
        monitor.Log($"Successfully added {npc.Name} as a companion to {farmer.Name}.", LogLevel.Trace);
    }

    /// <summary>
    /// Attempts to remove the npc as a companion to any farmer
    /// </summary>
    public void Remove(NPC npc)
    {
        IMonitor monitor = store.UseMonitor();
        
        // Early Exit: Cannot remove companion as NPC is not currently a companion
        if (!IsNpcCompanion(npc))
        {
            monitor.Log($"Could not remove {npc.Name} as a companion. {npc.Name} is not currently a companion!", LogLevel.Trace);
            return;
        }

        foreach (var entry in CurrentCompanions)
        {
            Companion? companion = entry.Value.Find(_companion => _companion.npc == npc);
            
            if (companion == null) 
                continue;
            
            entry.Value.Remove(companion);
            companion.Remove();
            return;
        }
    }
    
    /// <summary>
    /// Attempts to remove the npc as a companion to the provided farmer
    /// </summary>
    public void Remove(Farmer farmer, NPC npc)
    {
        IMonitor monitor = store.UseMonitor();
        
        // Early Exit: Cannot remove companion as NPC is not currently a companion for this farmer
        if (!IsNpcCompanionForFarmer(farmer, npc))
        {
            monitor.Log($"Could not remove {npc.Name} as a companion to {farmer.Name}. {npc.Name} is not currently a companion for {farmer.Name}!", LogLevel.Trace);
            return;
        }
        
        if (!CurrentCompanions.TryGetValue(farmer, out List<Companion>? companions)) 
            // Unreachable!
            // This should never happen because we already checked that CurrentCompanions has a value
            return;
        
        // Get companion from current companions list (this should always return a value because we checked above
        // that the npc is a valid companion for this farmer)
        Companion companion = companions.Find(companion => companion.npc == npc)!;
            
        companions.Remove(companion);
        companion.Remove();
    }
    
    
    public void DrawCompanions(Farmer farmer)
    {
        foreach (var entry in CurrentCompanions)
        {
            // If the selected row is not our current farmer skip it
            if (entry.Key != farmer)
            {
                continue;
            }

            foreach (Companion companion in entry.Value)
            {
                NPC npc = companion.npc;
                
                npc.position.X = (int)farmer.position.X;
                npc.position.Y = (int)farmer.position.Y;
            }
        }
        
        // For NPC in CompanionManager.CurrentCompanions
        // check if NPC farmer is current farmer
        // update NPC location to follow current farmer
    }
    
    /****
     ** Utility functions
     ****/

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
    
    public List<Companion> GetCurrentCompanions(Farmer farmer)
    {
        if (CurrentCompanions.TryGetValue(farmer, out List<Companion>? currentCompanions))
            return currentCompanions;
        
        List<Companion> newCompanions = new List<Companion>();
        
        CurrentCompanions.Add(farmer, newCompanions);
        return newCompanions;
    }

    public Companion? GetCompanion(NPC npc)
    {
        foreach (var entry in CurrentCompanions)
        {
            Companion? foundCompanion = entry.Value.Find(companion => companion.npc == npc);
            
            if (foundCompanion == null) 
                continue;
            
            return foundCompanion;
        }
        
        return null;
    }
    
    public bool IsNpcCompanion(NPC npc)
    {
        foreach (var entry in CurrentCompanions)
        {
            if (entry.Value.Any(companion => companion.npc == npc))
                return true;
        }

        return false;
    }

    public bool IsNpcCompanionForFarmer(Farmer farmer, NPC npc)
    {
        // Try to get companions for this farmer
        if (CurrentCompanions.TryGetValue(farmer, out List<Companion>? companions))
        {
            // Check if list of companions contains NPC
            return companions.Any(companion => companion.npc == npc);
        }

        // Farmer does not have any companions so this npc isn't currently a companion
        return false;
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

    /// <summary>
    /// When a farmer warps between maps get that players NPCs and update their location to be the same map as the
    /// farmer
    /// </summary>
    public void OnPlayerWarped(Farmer farmer, GameLocation newLocation)
    {
        List<Companion> companions = GetCurrentCompanions(farmer);
        
        foreach (Companion companion in companions)
        {
            companion.UpdateLocation(newLocation);
        }
    }

    public void OnUpdateTicking()
    {
        // Draw all NPCs
    }
}