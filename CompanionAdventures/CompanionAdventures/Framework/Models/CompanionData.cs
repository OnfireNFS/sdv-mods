using StardewValley;

namespace CompanionAdventures.Framework.Models;

public sealed class CompanionData(Farmer farmer, NPC npc)
{
    public Farmer Farmer = farmer;
    public NPC Npc = npc;
}