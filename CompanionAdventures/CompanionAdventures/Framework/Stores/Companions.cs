global using static CompanionAdventures.Framework.Companions;
using CompanionAdventures.Framework.Models;

namespace CompanionAdventures.Framework;

using StardewValley;

/// <summary>
/// Holds state for companions. This means any state change to a companion (add, remove, etc.) should happen through an
/// action provided by this store.
/// </summary>
public class Companions
{
    private static Companions? _instance;
    private Dictionary<string, Companion> _companions;

    private Companions()
    {
        // TODO: Load hearts and companions from config files
        this._companions = new Dictionary<string, Companion>
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
        // Early Exit: Check if NPC is already a companion
        if (companion.IsRecruited)
        {
            throw new CompanionAlreadyRecruitedException(companion.npc.Name);
        }
        
        // TODO:
        //  check if farmer has max companions
        // companion.StartFollowing(farmer);
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
        if (!companion.IsRecruited)
        {
            throw new CompanionNotRecruitedException(companion.npc.Name);
        }

        if (companion.Leader == farmer)
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
        return this._companions.TryGetValue(npc.Name, out companion);
    }
}