using CompanionAdventures.Framework.Models;
using StardewValley;
using static CompanionAdventures.Framework.Companions;

namespace CompanionAdventures.Framework;

public static class Interactions
{
    /// <summary>
    /// Handles when a NPC is interacted with
    /// </summary>
    /// <param name="farmer">The farmer interacting with the NPC</param>
    /// <param name="npc">The NPC being interacted with</param>
    public static void HandleInteraction(Farmer farmer, NPC npc)
    {
        Companions companions = UseCompanions();
        
        // Early Exit: If this npc is not a companion then return
        if (!companions.TryGetCompanion(npc, out Companion? companion))
        {
            return;
        }
        
        // Early Exit: Is this companion a valid companion for this farmer (check their heart level)
        if (!companion.IsCompanionValidForFarmer(farmer))
        {
            return;
        }
        
        // TODO: Is this companion recruitable?
            // AskToJoin(farmer, npc);
            
        // TODO: Is this companion following?
        //  if so is this farmer the leader?
            // AskOptions(farmer, npc);
    }
    
    // TODO: These need to not use Game1.currentLocation.createQuestionDialogue because that will ask all players in 
    //  this location, which isn't what we want
    /// <summary>
    /// Ask an NPC if they want to be a companion
    /// </summary>
    /// <param name="farmer">The farmer interacting with the NPC</param>
    /// <param name="npc">The NPC being interacted with</param>
    private static void AskToJoin(Farmer farmer, NPC npc)
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
                
                // store.Companions.Add(farmer, npc);
            }, 
            npc);
    }

    private static void AskOptions(Farmer farmer, NPC npc)
    {
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