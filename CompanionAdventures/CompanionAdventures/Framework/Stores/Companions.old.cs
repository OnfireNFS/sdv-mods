using CompanionAdventures.Framework.Models;

namespace CompanionAdventures.Framework;

public class CompanionsOld: StoreBase
{
    
    /****
     ** State
     ****/
    // Holds Farmers that are currently Leaders (meaning they have companions with them)
    private Dictionary<Farmer, Leader> _leaders = new ();
    
    public void Add(Farmer farmer, NPC npc)
    {
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
        Leader? leader = GetLeader(farmer);

        if (leader == null)
        {
            store.Monitor.Log($"Could not remove {npc.Name} as a companion. {farmer.Name} is not currently a leader!", LogLevel.Trace);
            return;
        }
        
        leader.RemoveCompanion(npc);

        if (leader.Companions.Count < 1)
        {
            RemoveLeader(farmer);
        }
        
        // Try to path this companion to its previous schedule location or warp it there
        ReturningCompanion? returningCompanion = ReturningCompanion.CreateReturningCompanionOrWarp(store, npc);
        
        // If we were able to generate a path to this companion's previous schedule location then store it as a 
        // returning companion
        if (returningCompanion != null)
            AddReturningCompanion(returningCompanion);
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

    public void AddReturningCompanion(ReturningCompanion returningCompanion)
    {
        if (_returningCompanions.ContainsKey(returningCompanion.npc))
        {
            store.Monitor.Log($"Attempted to add returning companion {returningCompanion.npc.Name} but {returningCompanion.npc.Name} is already a returning companion!", LogLevel.Warn);
            return;
        }
        
        _returningCompanions.Add(returningCompanion.npc, returningCompanion);
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
        store.Monitor.Log($"Checking if {npc.Name} can be a valid companion.", LogLevel.Trace);

        if (_validCompanions.Contains(npc.Name))
        {
            store.Monitor.Log($"{npc.Name} can be a companion.", LogLevel.Trace);
            return true;
        }
        
        store.Monitor.Log($"{npc.Name} can not be a companion.", LogLevel.Trace);
        return false;
    }
}