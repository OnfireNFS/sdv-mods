using StardewValley;

namespace CompanionAdventures.Framework;

/// <summary>
/// Static class that contains utility functions used by CompanionAdventures
/// </summary>
public static class Util
{
    /// <summary>
    /// Returns the number of hearts between the specified farmer and npc 
    /// </summary>
    /// <param name="farmer">Farmer to check friendship status of</param>
    /// <param name="npc">NPC to check friendship status of</param>
    /// <returns>Number of hearts as an integer</returns>
    public static int GetHeartLevel(Farmer farmer, NPC npc)
    {
        // Attempt to retrieve the farmers friendship value with the specified NPC
        if (farmer.friendshipData.TryGetValue(npc.Name, out Friendship friendship))
        {
            int friendshipPoints = friendship.Points;

            // The heart level is calculated based on friendship points (250 points per heart)
            int heartLevel = friendshipPoints / 250;
            
            return heartLevel;
        }
        
        // The player has no prior friendship with this NPC
        // The friendship level is 0 hearts.
        return 0;
    }
}