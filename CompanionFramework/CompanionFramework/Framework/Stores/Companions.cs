global using static CompanionFramework.Framework.Companions;
using CompanionFramework.Framework.Models;

namespace CompanionFramework.Framework;

using StardewValley;

/// <summary>
/// Holds state for companions. This means any state change to a companion (add, remove, etc.) should happen through an
/// action provided by this store.
/// </summary>
public class Companions
{
    private static Companions? _instance;
    private Dictionary<string, Companion> _availableCompanions;
    private Dictionary<Farmer, Leader> _leaders = new();

    private Companions()
    {
        // TODO: Load hearts and companions from config files
        this._availableCompanions = new Dictionary<string, Companion>
        {
            {"Abigail", new Companion(Game1.getCharacterFromName("Abigail")) },
        };
    }
    
    public static Companions UseCompanions()
    {
        return _instance ??= new Companions();
    }

    /// <summary>
    /// Adds the provided NPC as a companion to the provided Farmer
    /// </summary>
    /// <param name="farmer">Farmer instance to be leader</param>
    /// <param name="npc">NPC to follow farmer (must be a valid companion or exception will be thrown)</param>
    /// <exception cref="CompanionNotFoundException">Thrown if the provided NPC is not a valid companion</exception>
    public void Add(Farmer farmer, NPC npc)
    {
        // Early Exit: If this npc is not a companion
        if (!TryGetCompanion(npc, out Companion? companion))
        {
            throw new CompanionNotFoundException(npc.Name);
        }
        
        Add(farmer, companion!);
    }

    public void Add(Farmer farmer, Companion companion)
    {
        Resources resources = UseResources();

        // Early Exit: Cannot add a companion if the max companions is 0 or less
        if (resources.Config.MaxCompanions < 1)
        {
            resources.Monitor.Log($"Cannot add {companion.npc.Name} as a companion, the configured maximum number of companions for a farmer is less than 1");
            return;
        }
        
        // Early Exit: Check if NPC is already a companion
        if (companion.Availability.Value == CompanionAvailability.Recruited)
        {
            throw new CompanionAlreadyRecruitedException(companion.npc.Name);
        }

        if (!_leaders.TryGetValue(farmer, out Leader? leader))
        {
            leader = new Leader(farmer);
            _leaders.Add(farmer, leader);
        }

        // Early Exit: Cannot add a companion if the farmer already has the maximum number of companions
        if (leader.Companions.Count >= resources.Config.MaxCompanions)
        {
            resources.Monitor.Log($"Cannot add {companion.npc.Name} as a companion, {farmer.Name} already has the maximum number of companions");
            return;
        }

        leader.AddCompanion(companion);
        resources.Monitor.Log($"{companion.npc.Name} is now a companion for {farmer.Name}");
    }

    public void Remove(Farmer farmer, NPC npc)
    {
        // Early Exit: If this npc is not a companion
        if (!TryGetCompanion(npc, out Companion? companion))
        {
            throw new CompanionNotFoundException(npc.Name);
        }
        
        Remove(farmer, companion!);
    }

    public void Remove(Farmer farmer, Companion companion)
    {
        // Early Exit: If this companion is not recruited
        if (companion.Availability.Value != CompanionAvailability.Recruited)
        {
            throw new CompanionNotRecruitedException(companion.npc.Name);
        }

        if (companion.Leader.Farmer == farmer)
        {
            // companion.StopFollowing();
        }
        else
        {
            throw new CompanionNotFollowingFarmerException(companion.npc.Name, farmer.Name);
        }
    }
    
    public bool TryGetCompanion(NPC npc, out Companion? companion)
    {
        return this._availableCompanions.TryGetValue(npc.Name, out companion);
    }
    
    /// <summary>
    /// Initialize companions for a new day
    /// </summary>
    public void OnDayStarted()
    {
        foreach (var (_, companion) in _availableCompanions)
        {
            companion.OnDayStarted();
        }
    }
}