using StardewModdingAPI;
using StardewValley;
using static CompanionAdventures.Framework.Resources;

namespace CompanionAdventures.Framework.Models;

/// <summary>
/// Different states a companion can be in
/// Unavailable: Companion cannot be recruited
/// Available: Companion is available to be recruited
/// Recruited: Companion is currently recruited
/// Returning: Companion is returning to previous location
/// </summary>
enum CompanionAvailability
{
    Unavailable,
    Available,
    Recruited
}

public class Companion
{
    private readonly int _heartThreshold = 0;
    private CompanionAvailability _availability = CompanionAvailability.Unavailable;
    
    public NPC npc;
    public Farmer? Leader = null;

    public bool IsAvailable => this._availability == CompanionAvailability.Available;
    public bool IsUnavailable => this._availability == CompanionAvailability.Unavailable;
    public bool IsRecruited => this._availability == CompanionAvailability.Recruited;

    public bool IsCompanionValidForFarmer(Farmer farmer)
    {
        Resources resources = UseResources();
        resources.Monitor.Log($"Checking if {npc.Name} can be a valid companion for {farmer.Name}.");
        
        // Get the heart level of the farmer and this npc
        var hearts = Util.GetHeartLevel(farmer, npc);

        // Return true if number of hearts is equal to or above heart threshold
        if (hearts >= _heartThreshold)
        {
            resources.Monitor.Log($"{npc.Name} can be a valid companion for {farmer.Name}.", LogLevel.Trace);
            return true;
        }
    
        resources.Monitor.Log($"{npc.Name} is not a valid companion for {farmer.Name}.", LogLevel.Trace);
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
        // Early Exit: If companion isn't available then return
        if (!IsAvailable)
        {
            return;
        }
        
        // Early Exit: Is this companion a valid companion for this farmer (check their heart level)
        if (!IsCompanionValidForFarmer(farmer))
        {
            return;
        }
        
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
                
                // store.Companions.Add(farmer, npc);
            }, 
            npc);
    }

    public void AskOptions()
    {
        // Early Exit: If companion isn't recruited then return
        if (!IsRecruited)
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
            (Farmer _farmer, string response) =>
            {
                // Early Exit: If farmer decided not to ask NPC to leave then do nothing
                if (response == Constants.DialogReject)
                {
                    return;
                }
                
                // store.Companions.Remove(farmer, npc);
            }, 
            npc);
    }
}