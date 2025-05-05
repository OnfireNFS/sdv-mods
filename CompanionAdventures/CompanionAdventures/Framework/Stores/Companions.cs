using CompanionAdventures.Framework.Models;

namespace CompanionAdventures.Framework;

using StardewModdingAPI;
using StardewValley;

#region Store Setup
public partial class Store
{
    public Companions UseCompanions()
    {
        return DefineStore<Companions>();
    }
}
#endregion

/// <summary>
/// Holds functions for creating and removing companions as well as some utility functions for determining if npcs are
/// valid companions. Also handles creating and removing Leader and Companion classes automatically
/// </summary>
public class Companions: IStore
{
    public required Store store { get; set; }
    
    /****
     ** Config
     ****/
    private List<string> _validCompanions;
    private Dictionary<string, int> _companionHeartsThreshold;
    
    /****
     ** State
     ****/
    // Holds Farmers that are currently Leaders (meaning they have companions with them)
    private Dictionary<Farmer, Leader> _leaders = new ();
    // Holds NPCs that are no longer Companions but are not managed by the game yet because they are returning to their
    // regular schedule location
    private Dictionary<NPC, ReturningCompanion> _returningCompanions = new ();

    public Companions()
    {
        // TODO: Load hearts and companions from config files
        
        this._validCompanions = new List<string> {"Abigail", "Penny"};
        this._companionHeartsThreshold = new Dictionary<string, int> {{"Abigail", 0}, {"Penny", 0 }};
    }
    
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

        if (_returningCompanions.TryGetValue(npc, out ReturningCompanion? returningCompanion))
        {
            RemoveReturningCompanion(npc);
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
        _leaders.Add(farmer, leader);
        return leader;
    }

    private Leader? GetLeader(Farmer farmer)
    {
        if (_leaders.TryGetValue(farmer, out Leader? leader))
            return leader;
        
        return null;
    }
    
    private void RemoveLeader(Farmer farmer)
    {
        Leader? leader = GetLeader(farmer);

        // Early Exit: If farmer isn't a leader there is no leader to remove
        if (leader == null) 
            return;
        
        _leaders.Remove(farmer);
        leader.Remove();
    }

    public void AddReturningCompanion(NPC npc)
    {
        if (_returningCompanions.ContainsKey(npc))
        {
            IMonitor monitor = store.UseMonitor();
            monitor.Log($"Attempted to add returning companion {npc.Name} but {npc.Name} is already a returning companion!", LogLevel.Warn);
            return;
        }
        
        _returningCompanions.Add(npc, new ReturningCompanion(store, npc));
    }

    private ReturningCompanion? GetReturningCompanion(NPC npc)
    {
        if (_returningCompanions.TryGetValue(npc, out ReturningCompanion? returningCompanion))
            return returningCompanion;
        
        return null;
    }
    
    public void RemoveReturningCompanion(NPC npc)
    {
        ReturningCompanion? returningCompanion = GetReturningCompanion(npc);

        // Early Exit: If npc isn't a ReturningCompanion there is no ReturningCompanion to remove
        if (returningCompanion == null)
            return;
        
        _returningCompanions.Remove(npc);
        returningCompanion.Remove();
    }
    
    /****
     ** Utility functions
     ****/

    public bool IsNpcValidCompanion(NPC npc)
    {
        IMonitor monitor = store.UseMonitor();
        
        monitor.Log($"Checking if {npc.Name} can be a valid companion.", LogLevel.Trace);

        if (_validCompanions.Contains(npc.Name))
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
        if (!_companionHeartsThreshold.TryGetValue(npc.Name, out int companionHearts))
        {
            monitor.Log($"Could not get heart level requirement for {npc.Name}!", LogLevel.Trace);
            return false;
        }
        var hearts = Util.GetHeartLevel(farmer, npc);

        // Return true if number of hearts is equal to or above heart threshold
        if (hearts >= companionHearts)
        {
            monitor.Log($"{npc.Name} can be a valid companion for {farmer.Name}.", LogLevel.Trace);
            return true;
        }
        
        monitor.Log($"{npc.Name} is not a valid companion for {farmer.Name}.", LogLevel.Trace);
        return false;

    }
    
    public bool IsNpcCompanion(NPC npc, out Farmer? farmer)
    {
        foreach (var entry in _leaders)
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