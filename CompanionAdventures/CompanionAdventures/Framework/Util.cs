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
    /// Returns the first NPC that the cursor is currently over
    /// </summary>
    /// <param name="cursor">Cursor to use for grabbing the current Tile</param>
    /// <returns>
    /// First NPC found within the 64x64 tile that the cursor is currently over. If no NPC is found then returns null.
    /// </returns>
    /// https://github.com/spacechase0/StardewValleyMods/blob/develop/AdvancedSocialMenu/Mod.cs#L72-88
    public static NPC? GetFirstNpcFromCursor(ICursorPosition cursor)
    {
        Rectangle area = new Rectangle((int)cursor.GrabTile.X * 64, (int)cursor.GrabTile.Y * 64, 64, 64);
        NPC? npc = null;
        
        // Get the first non-monster npc inside the rectangle
        foreach (var character in Game1.currentLocation.characters)
        {
            if (!character.IsMonster && character.GetBoundingBox().Intersects(area))
            {
                npc = character;
                break;
            }
        }
        // Alternative ways to grab the npc
        if (npc == null)
            npc = Game1.currentLocation.isCharacterAtTile(cursor.Tile + new Vector2(0f, 1f));
        if (npc == null)
            npc = Game1.currentLocation.isCharacterAtTile(cursor.GrabTile + new Vector2(0f, 1f));

        return npc;
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