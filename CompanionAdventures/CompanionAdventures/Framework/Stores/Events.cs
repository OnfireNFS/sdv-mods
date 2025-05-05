using CompanionAdventures.Framework.Models;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace CompanionAdventures.Framework;

#region Store Setup
public partial class Store
{
    public Events Events => DefineStore<Events>();
}
#endregion

/// <summary>
/// Holds global events that are not specific to a leader or companion
///
/// For example: handling controller input or loading assets
/// </summary>
public class Events: StoreBase
{
    /****
     ** Events
     ****/
    public void RegisterEvents()
    {
        store.Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        store.Helper.Events.Input.ButtonPressed += OnButtonPressed;
    }
    
    /// <summary>
    /// Handles "OnButtonPressed" events for the CompanionAdventures mod.
    ///
    /// More specifically will check if the button pressed was the action button, if the farmer is trying to
    /// interact with a NPC and if the NPC has interactions relating to CompanionAdventures.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    public void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        // Ignore if player isn't in the world or if they are in a cutscene
        if (!Context.IsPlayerFree)
            return;

        // Check if the button pressed is the interact button
        if (e.Button.IsActionButton())
        {
            // Get the first NPC in the tile that the cursor is currently over
            NPC? npc = Util.GetFirstNpcFromCursor(e.Cursor);

            // Early Exit: If there is no NPC there is nothing to do
            if (npc == null)
                return;
            
            // Update the currentDialogue in case it is outdated
            var farmer = Game1.player;
            var heartLevel = Util.GetHeartLevel(farmer, npc);
            npc.checkForNewCurrentDialogue(heartLevel);
            
            // If NPC is currently not a companion and has a Dialogue message queued --or--
            // NPC cannot be a companion for this farmer:
            // Do nothing and let the dialogue message trigger or default behaviour trigger
            if (npc.CurrentDialogue.Count > 0)
            {
                return;
            }
            
            // Suppress default behavior 
            store.Helper.Input.Suppress(e.Button);
            
            // Use the interactions store to handle interacting with this NPC
            store.Interactions.HandleInteraction(farmer, npc);
        }
    }
    
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var api = store.Helper.ModRegistry.GetApi<IContentPack>("Pathoschild.ContentPatcher");
    }
}