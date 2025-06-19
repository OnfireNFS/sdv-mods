using StardewModdingAPI;
using StardewValley;

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
                
                Companions companions = UseCompanions();
                
                // companions.Remove(farmer, npc);
            }, 
            npc);
    }
}