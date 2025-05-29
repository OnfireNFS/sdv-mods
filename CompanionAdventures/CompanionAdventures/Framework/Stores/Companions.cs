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

    private Companions() { }
    
    public static Companions UseCompanions()
    {
        return _instance ??= new Companions();
    }

    public bool TryGetCompanion(NPC npc, out Companion companion)
    {
        companion = null;
        
        return false;
    }
}