using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace CompanionAdventures.Framework;

/// <summary>
/// Static class that contains utility functions used by CompanionAdventures
/// </summary>
public static class Util
{
    /// <summary>
    /// Returns the Tile that the cursor is currently over
    /// </summary>
    /// <param name="cursor">Cursor to use for grabbing the Tile</param>
    /// <returns>64x64 Tile that the cursor is currently in</returns>
    public static Rectangle GetCursorTile(ICursorPosition cursor)
    {
        return  new Rectangle((int)cursor.GrabTile.X * 64, (int)cursor.GrabTile.Y * 64, 64, 64);
    }
    
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