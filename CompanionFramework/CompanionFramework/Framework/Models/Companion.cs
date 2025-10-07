using Microsoft.Xna.Framework;
using StardewValley;

namespace CompanionFramework.Framework.Models;

/// <summary>
/// Different states a companion can be in
/// Unavailable: Companion cannot be recruited
/// Available: Companion is available to be recruited
/// Recruited: Companion is currently recruited
/// Returning: Companion is returning to previous location
/// </summary>
public enum CompanionAvailability
{
    Unavailable,
    Available,
    Recruited
}

public class Companion
{
    private readonly int _heartThreshold = 0;
    
    public Ref<CompanionAvailability> Availability = new(CompanionAvailability.Unavailable);
    public NPC npc;
    public Leader? Leader = null;
    public Companion(NPC npc)
    {
        this.npc = npc;
    }
    
    public bool IsCompanionValidForFarmer(Farmer farmer)
    {
        Resources resources = UseResources();
        
        resources.Monitor.Log($"Checking if {npc.Name} can be a valid companion for {farmer.Name}.");
        
        // Get the heart level of the farmer and this npc
        var hearts = Util.GetHeartLevel(farmer, npc);

        // Return true if number of hearts is equal to or above heart threshold
        if (hearts >= _heartThreshold)
        {
            resources.Monitor.Log($"{npc.Name} can be a valid companion for {farmer.Name}.");
            return true;
        }
    
        resources.Monitor.Log($"{npc.Name} is not a valid companion for {farmer.Name}.");
        return false;
    }
    
    /****
     ** Dialogue
     ****/
    /// <summary>
    /// Ask this companion to follow the provided farmer
    /// </summary>
    /// <param name="farmer">Farmer asking companion to follow them</param>
    public void AskToJoin(Farmer farmer)
    {
        // TODO: Translation
        string dialogText = $"Ask {npc.Name} to follow?";
        Response[] responses =
        [
            new Response(Constants.DialogApprove, "Yes"),
            new Response(Constants.DialogReject, "No"),
        ];

        // This probably shows dialogue for all players in this location
        Game1.currentLocation.createQuestionDialogue(dialogText, responses,
            (Farmer _farmer, string response) =>
            {
                // Early Exit: If farmer decided not to ask NPC to become companion then do nothing
                if (response == Constants.DialogReject)
                {
                    return;
                }
                Companions companions = UseCompanions();
                
                companions.Add(farmer, npc);
            }, 
            npc);
    }

    public void AskOptions()
    {
        // Early Exit: If companion isn't recruited then return
        if (this.Availability.Value != CompanionAvailability.Recruited)
        {
            return;
        }
        
        string dialogText = $"Ask {npc.Name} to leave?";
        Response[] responses =
        [
            new Response(Constants.DialogApprove, "Yes"),
            new Response(Constants.DialogReject, "No"),
        ];
        
        Game1.currentLocation.createQuestionDialogue(dialogText, responses,
            (Farmer farmer, string response) =>
            {
                // Early Exit: If farmer decided not to ask NPC to leave then do nothing
                if (response == Constants.DialogReject)
                {
                    return;
                }
                
                Companions companions = UseCompanions();
                
                companions.Remove(farmer, npc);
            }, 
        npc);
    }

    public void StartFollowing(Leader leader)
    {
        Leader = leader;
        var stopLocation = Watch(leader.Location, UpdateLocation);
        var stopTitle = Watch(leader.Tile, UpdateTile);
    }
    
    private void UpdateTile(Vector2 tile)
    {
        npc.position.X = (int)tile.X * 64;
        npc.position.Y = (int)tile.Y * 64;
    }
    
    public void UpdateLocation(GameLocation newLocation)
    {
        // Don't warp this Companion if they are already on this map
        if(npc.currentLocation.Equals(newLocation))
            return;
        
        Game1.warpCharacter(npc, newLocation, Leader.Tile.Value);
    }
    
    /****
     ** Events
     ****/
    public void OnDayStarted()
    {
        Resources resources = UseResources();
        resources.Monitor.Log($"{npc.Name} is a valid companion today");
        
        this.Availability.Value = CompanionAvailability.Available;
    }
}