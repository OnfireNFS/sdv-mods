using CompanionAdventures.Framework;
using CompanionAdventures.Framework.Models;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Companions;
using Constants = CompanionAdventures.Framework.Constants;

namespace CompanionAdventures
{
    public class CompanionAdventures : Mod
    {
        /****
         ** State
         ****/
        /// <summary>The mod settings.</summary>
        private ModConfig Config = null!;
        
        /// <summary>Manages companions for this player</summary>
        private CompanionManager CompanionManager = null!;

        /// <summary>
        /// The mod entry point, called after the mod is first loaded.
        /// </summary>
        public override void Entry(IModHelper helper)
        {
            // Initialize state
            this.Config = Helper.ReadConfig<ModConfig>();

            this.CompanionManager =  new CompanionManager(
                this.Config, 
                this.ModManifest, 
                helper.Multiplayer, 
                this.Monitor
            );
            
            // Hook events
            IModEvents events = helper.Events;
            events.GameLoop.GameLaunched += OnGameLaunched;
            events.GameLoop.UpdateTicking += OnUpdateTicking;
            events.Input.ButtonPressed += OnButtonPressed;
            events.Multiplayer.ModMessageReceived += OnMessageReceived;
        }
        
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var api = this.Helper.ModRegistry.GetApi<IContentPack>("Pathoschild.ContentPatcher");
        }

        private void OnUpdateTicking(object? sender, UpdateTickingEventArgs e)
        {
            CompanionManager.DrawCompanions(Game1.player);
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
                
                // Update the currentDialogue in case it is outdated
                var farmer = Game1.player;
                var heartLevel = Util.GetHeartLevel(farmer, npc);
                npc.checkForNewCurrentDialogue(heartLevel);
                
                // If NPC is currently not a companion and has a Dialogue message queued --or--
                // NPC cannot be a companion for this farmer:
                // Do nothing and let the dialogue message trigger or default behaviour trigger
                if (npc.CurrentDialogue.Count > 0 || !CompanionManager.IsNpcValidCompanionForFarmer(farmer, npc))
                {
                    return;
                }

                if (CompanionManager.IsCurrentlyCompanionForFarmer(farmer, npc))
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
                            Helper.Input.Suppress(e.Button);
                            CompanionManager.AddCompanion(farmer, npc);
                        
                            Monitor.Log($"Is {npc.Name} a companion? {CompanionManager.IsCompanion(npc)}");
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
                            Helper.Input.Suppress(e.Button);
                            CompanionManager.AddCompanion(farmer, npc);
                        
                            Monitor.Log($"Is {npc.Name} a companion? {CompanionManager.IsCompanion(npc)}");
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
        
        private void OnMessageReceived(object? sender, ModMessageReceivedEventArgs e)
        {
            // Early Exit: The message received was from a different mod
            if (e.FromModID != ModManifest.UniqueID)
            {
                return;
            }

            if (e.Type == Constants.MessagetypeCompanionAdded)
            {
                CompanionManager.OnCompanionAdded(e.ReadAs<CompanionData>());
            } else if (e.Type == Constants.MessagetypeCompanionRemoved)
            {
                CompanionManager.OnCompanionRemoved(e.ReadAs<CompanionData>());
            } else if (e.Type == Constants.MessagetypeCompanionUpdated)
            {
                CompanionManager.OnCompanionUpdated(e.ReadAs<CompanionData>());
            }
            else
            {
                string data = e.ReadAs<string>();
                Monitor.Log($"Received unknown event type: \"{e.Type}\" event with data: \"{data}\"", LogLevel.Error);
            }
        }
    }
}