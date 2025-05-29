using StardewValley;

namespace CompanionAdventures.Framework.Models;

public class Companion
{
    public NPC npc;
    public Farmer? leader = null;
    
    public bool IsCompanionValidForFarmer(Farmer farmer)
    {
        return false;
    }
}