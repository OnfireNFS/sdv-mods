using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace CompanionAdventures.Framework;

public class Events
{
    /// <summary>
    /// Register the events that the CompanionAdventures mod listens to and what functions should be called to
    /// handle those events
    /// </summary>
    public static void RegisterEvents()
    {
        /*
         These events will most likely be required by this mod, but currently are just stubs

        events.GameLoop.SaveLoaded += new EventHandler<SaveLoadedEventArgs>((object sender, SaveLoadedEventArgs e) => {});
        events.GameLoop.Saving += new EventHandler<SavingEventArgs>((object sender, SavingEventArgs e) => {});
        events.GameLoop.ReturnedToTitle += new EventHandler<ReturnedToTitleEventArgs>((object sender, ReturnedToTitleEventArgs e) => {});
        events.GameLoop.DayStarted += new EventHandler<DayStartedEventArgs>((object sender, DayStartedEventArgs e) => {});
        events.GameLoop.DayEnding += new EventHandler<DayEndingEventArgs>((object sender, DayEndingEventArgs e) => {});
        events.GameLoop.UpdateTicked += new EventHandler<UpdateTickedEventArgs>((object sender, UpdateTickedEventArgs e) => {});
        */
        CompanionAdventures mod = Stores.useMod();
        IModEvents events = mod.Helper.Events;
            
        events.GameLoop.GameLaunched += OnGameLaunched;
        events.Input.ButtonPressed += OnButtonPressed;
        events.Multiplayer.ModMessageReceived += OnMessageReceived;
    }
    
    public static void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        CompanionAdventures mod = Stores.useMod();
        var api = mod.Helper.ModRegistry.GetApi<IContentPack>("Pathoschild.ContentPatcher");
    }
    
    /// <summary>
    /// Handles "OnButtonPressed" events for the CompanionAdventures mod.
    ///
    /// More specifically will check if the button pressed was the action button, if the farmer is trying to
    /// interact with a NPC and if the NPC has interactions relating to CompanionAdventures.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event data.</param>
    public static void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        // Ignore if player isn't in the world or if they are in a cutscene
        if (!Context.IsPlayerFree)
            return;

        // Check if the button pressed is the interact button
        if (e.Button.IsActionButton())
        {
            // https://github.com/spacechase0/StardewValleyMods/blob/develop/AdvancedSocialMenu/Mod.cs#L72-88
            
            // Get the tile that the cursor is currently in to scan for NPCs
            Rectangle currentTile = Util.GetCursorTile(e.Cursor);
            
            NPC? npc = null;
            // Get the first non-monster npc inside the rectangle
            foreach (var character in Game1.currentLocation.characters)
            {
                if (!character.IsMonster && character.GetBoundingBox().Intersects(currentTile))
                {
                    npc = character;
                    break;
                }
            }
            // Alternative ways to grab the npc
            if (npc == null)
                npc = Game1.currentLocation.isCharacterAtTile(e.Cursor.Tile + new Vector2(0f, 1f));
            if (npc == null)
                npc = Game1.currentLocation.isCharacterAtTile(e.Cursor.GrabTile + new Vector2(0f, 1f));

            // Early Exit: If there is no NPC there is nothing to do
            if (npc == null)
                return;

            var companionManager = Stores.useCompanionManager();
            
            // Update the currentDialogue in case it is outdated
            var farmer = Game1.player;
            var heartLevel = Util.GetHeartLevel(farmer, npc);
            npc.checkForNewCurrentDialogue(heartLevel);
            
            // If NPC is currently not a companion and has a Dialogue message queued --or--
            // NPC cannot be a companion for this farmer:
            // Do nothing and let the dialogue message trigger or default behaviour trigger
            if (npc.CurrentDialogue.Count > 0 || !companionManager.IsNpcValidCompanionForFarmer(farmer, npc))
            {
                return;
            }
            
            CompanionAdventures mod = Stores.useMod();
            IModHelper helper = mod.Helper;
            IMonitor monitor = mod.Monitor;

            if (companionManager.IsCurrentlyCompanionForFarmer(farmer, npc))
            {
                string dialogText = $"Ask {npc.Name} to leave?";
                Response[] responses =
                [
                    new Response("yes_key", "Yes"),
                    new Response("no_key", "No"),
                ];
                
                void AfterQuestionBehaviour(Farmer farmer, string responseText)
                {
                    if (responseText == "yes_key")
                    {
                        // Prevent default behaviour
                        helper.Input.Suppress(e.Button);
                        companionManager.AddCompanion(farmer, npc);
                    
                        monitor.Log($"Is {npc.Name} a companion? {companionManager.IsCompanion(npc)}");
                    }
                    else
                    {
                    
                    }
                }

                Game1.currentLocation.createQuestionDialogue(dialogText, responses, AfterQuestionBehaviour, npc);
            }
            else
            {
                // Create dialog options
                string dialogText = $"Ask {npc.Name} to follow?";
                Response[] responses =
                [
                    new Response("yes_key", "Yes"),
                    new Response("no_key", "No"),
                ];
            
                void AfterQuestionBehaviour(Farmer farmer, string responseText)
                {
                    if (responseText == "yes_key")
                    {
                        // Prevent default behaviour
                        helper.Input.Suppress(e.Button);
                        companionManager.AddCompanion(farmer, npc);
                    
                        monitor.Log($"Is {npc.Name} a companion? {companionManager.IsCompanion(npc)}");
                    }
                    else
                    {
                    
                    }
                }

                Game1.currentLocation.createQuestionDialogue(dialogText, responses, AfterQuestionBehaviour, npc);
            }
        }
        
        // print button presses to the console window
        // this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}. {Native.Version()}", LogLevel.Debug);
        // this.Monitor.Log($"{Native.SayHello(Game1.player.Name)}, you pressed {e.Button}.", LogLevel.Debug);
    }
    
    public static void OnMessageReceived(object? sender, ModMessageReceivedEventArgs e)
    {
        CompanionAdventures mod = Stores.useMod();
        IManifest modManifest = mod.ModManifest;
        IMonitor monitor = mod.Monitor;
        
        // Early Exit: The message received was from a different mod
        if (e.FromModID != modManifest.UniqueID)
        {
            return;
        }
        
        Multiplayer multiplayer = Stores.useMultiplayer();
        multiplayer.ReceiveMessage(e);
    }
}