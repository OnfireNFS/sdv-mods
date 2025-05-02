using CompanionAdventures.Framework.Models;

namespace CompanionAdventures.Framework;

using StardewModdingAPI;
using StardewValley;

#region Store Setup
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
#endregion

/// <summary>
/// Manages creating and removing companions for this player
/// </summary>
public class Companions
{
    #region Store Setup
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
    #endregion

    /****
     ** Companions
     ****/
    public Dictionary<Farmer, Leader> CurrentLeaders = new ();
    
    public int CompanionHeartsThreshold = 0;
    public List<string> ValidCompanions = new List<string> {"Abigail", "Penny"};

    /// <summary>
    /// Attempts to add the npc as a companion to the provided farmer
    /// </summary>
    public void Add(Farmer farmer, NPC npc)
    {
        IMonitor monitor = store.UseMonitor();

        // Early Exit: Check if NPC is already a companion
        if (IsNpcCompanion(npc, out Farmer? currentFarmer))
        {
            // Companion existingCompanion = GetCompanion(npc)!;
            monitor.Log($"Could not add {npc.Name} as a companion to {farmer.Name}. {npc.Name} is already a companion for {currentFarmer!.Name}!", LogLevel.Trace);
            return;
        }
        
        // Early Exit: Check if NPC can be a companion for this farmer
        if (!IsNpcValidCompanionForFarmer(npc, farmer))
        {
            monitor.Log($"Could not add {npc.Name} as a companion to {farmer.Name}. {npc.Name} is not a valid companion for {farmer.Name}!", LogLevel.Trace);
            return;
        }

        Leader leader = GetLeader(farmer) ?? CreateLeader(farmer);
        leader.AddCompanion(npc);
    }
    
    /// <summary>
    /// Attempts to remove the npc as a companion to the provided farmer
    /// </summary>
    public void Remove(Farmer farmer, NPC npc)
    {
        IMonitor monitor = store.UseMonitor();

        Leader? leader = GetLeader(farmer);

        if (leader == null)
        {
            monitor.Log($"Could not remove {npc.Name} as a companion. {farmer.Name} is not currently a leader!", LogLevel.Trace);
            return;
        }
        
        leader.RemoveCompanion(npc);

        if (leader.Companions.Count < 1)
        {
            RemoveLeader(farmer);
        }
    }

    private Leader CreateLeader(Farmer farmer)
    {
        Leader leader = new Leader(store, farmer);
        CurrentLeaders.Add(farmer, leader);
        return leader;
    }

    public Leader? GetLeader(Farmer farmer)
    {
        if (CurrentLeaders.TryGetValue(farmer, out Leader? leader))
            return leader;
        
        return null;
    }
    
    private void RemoveLeader(Farmer farmer)
    {
        Leader? leader = GetLeader(farmer);

        // Early Exit: If farmer isn't a leader there is no leader to remove
        if (leader == null) 
            return;
        
        CurrentLeaders.Remove(farmer);
        leader.Dispose();
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

    public bool IsNpcValidCompanionForFarmer(NPC npc, Farmer farmer)
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
    
    public bool IsNpcCompanion(NPC npc, out Farmer? farmer)
    {
        foreach (var entry in CurrentLeaders)
        {
            if (entry.Value.IsCompanion(npc))
            {
                farmer = entry.Key;
                return true;
            }
        }

        farmer = null;
        return false;
    }

    public bool IsNpcCompanionForFarmer(Farmer farmer, NPC npc)
    {
        Leader? leader = GetLeader(farmer);
        
        if (leader != null)
            return leader.IsCompanion(npc);
        
        return false;
    }
}