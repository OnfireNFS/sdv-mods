using CompanionAdventures.Framework.Models;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace CompanionAdventures.Framework;

public partial class Store
{
    private Events? _events = null;

    public void _Events(Events events)
    {
        _events ??= events;
    }

    public Events UseEvents()
    {
        if (_events == null)
            Events.CreateStore(this);

        return _events!;
    }
}

public class Events
{
    /****
     ** Store Setup
     ****/
    private Store store;
    private Events(Store store)
    {
        this.store = store;
    }
    public static void CreateStore(Store store)
    {
        store._Events(new Events(store));
    }
    
    /****
     ** Events
     ****/

    public void RegisterEvents()
    {
        IModHelper helper = store.UseHelper();
        IModEvents events = helper.Events;
        
        events.GameLoop.GameLaunched += OnGameLaunched;
        events.GameLoop.UpdateTicking += OnUpdateTicking;
        events.Input.ButtonPressed += OnButtonPressed;
        events.Multiplayer.ModMessageReceived += OnMessageReceived;
        events.Player.Warped += OnPlayerWarped;
    }
    
    /// <summary>
    /// Handles "OnButtonPressed" events for the CompanionAdventures mod.
    ///
    /// More specifically will check if the button pressed was the action button, if the farmer is trying to
    /// interact with a NPC and if the NPC has interactions relating to CompanionAdventures.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
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
            IModHelper helper = store.UseHelper();
            helper.Input.Suppress(e.Button);
            
            // Use the interactions store to handle interacting with this NPC
            Interactions interactions = store.UseInteractions();
            interactions.HandleInteraction(farmer, npc);
        }
    }
    
    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        IModHelper helper = store.UseHelper();
        
        var api = helper.ModRegistry.GetApi<IContentPack>("Pathoschild.ContentPatcher");
    }
    
    private void OnMessageReceived(object? sender, ModMessageReceivedEventArgs e)
    {
        IManifest modManifest = store.UseManifest();
        
        // Early Exit: The message received was from a different mod
        if (e.FromModID != modManifest.UniqueID)
        {
            return;
        }
        
        Companions companions = store.UseCompanions();
        IMonitor monitor = store.UseMonitor();

        switch (e.Type)
        {
            case Constants.MessagetypeCompanionAdded:
                companions.OnCompanionAdded(e.ReadAs<CompanionData>());
                break;
            case Constants.MessagetypeCompanionRemoved:
                companions.OnCompanionRemoved(e.ReadAs<CompanionData>());
                break;
            case Constants.MessagetypeCompanionUpdated:
                companions.OnCompanionUpdated(e.ReadAs<CompanionData>());
                break;
            default:
            {
                string data = e.ReadAs<string>();
                monitor.Log($"Received unknown event type: \"{e.Type}\" event with data: \"{data}\"", LogLevel.Error);
                break;
            }
        }
    }

    private void OnPlayerWarped(object? sender, WarpedEventArgs e)
    {
        Companions companions = store.UseCompanions();
        
        companions.OnPlayerWarped(e.Player, e.NewLocation);
    }
    
    private void OnUpdateTicking(object? sender, UpdateTickingEventArgs e)
    {
        Companions companions = store.UseCompanions();

        // On each tick update the position of each farmer that has companions
        foreach (var entry in companions.CurrentCompanions)
        {
            Leader leader = entry.Key;
            leader.UpdateTile(leader.Farmer.Tile);
        }
    }
}