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
            {"Abigail", new Companion()}
        };
    }
    
    public static Companions UseCompanions()
    {
        return _instance ??= new Companions();
    }

    public void Add(Farmer farmer, NPC npc)
    {
        // Early Exit: If this npc is not a companion then return
        if (!TryGetCompanion(npc, out Companion companion))
        {
            throw new InvalidCompanionException;
        }
        
        Add(farmer, companion);
    }

    public void Add(Farmer farmer, Companion companion)
    {
        
    }
    
    public bool TryGetCompanion(NPC npc, out Companion companion)
    {
        companion = null;
        
        return false;
    }
}