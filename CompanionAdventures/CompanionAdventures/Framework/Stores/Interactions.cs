using StardewValley;

namespace CompanionAdventures.Framework;

#region Store Setup
public partial class Store
{
    public Interactions UseInteractions()
    {
        return DefineStore<Interactions>();
    }
}
#endregion

/// <summary>
/// Holds global events that are not specific to a leader or companion
///
/// For example: handling controller input or loading assets
/// </summary>
public class Interactions: IStore
{
    public required Store store { get; set; }
    
    /// <summary>
    /// Handles when a NPC is interacted with
    /// </summary>
    /// <param name="farmer">The farmer interacting with the NPC</param>
    /// <param name="npc">The NPC being interacted with</param>
    public void HandleInteraction(Farmer farmer, NPC npc)
    {
        Companions companions = store.UseCompanions();
        
        // Early Exit: If this npc cannot be a companion then return
        if (!companions.IsNpcValidCompanion(npc))
        {
            return;
        }
        
        // Is the NPC being interacted with currently a companion?
        if (companions.IsNpcCompanionForFarmer(farmer, npc))
        {
            AskOptions(farmer, npc);
        }
        else
        {
            AskToJoin(farmer, npc);
        }
    }

    /// <summary>
    /// Ask an NPC if they want to be a companion
    /// </summary>
    /// <param name="farmer">The farmer interacting with the NPC</param>
    /// <param name="npc">The NPC being interacted with</param>
    public void AskToJoin(Farmer farmer, NPC npc)
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
                
                Companions companions = store.UseCompanions();
                companions.Add(farmer, npc);
            }, 
            npc);
    }

    public void AskOptions(Farmer farmer, NPC npc)
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
                
                Companions companions = store.UseCompanions();
                companions.Remove(farmer, npc);
            }, 
            npc);
    }
    
}
