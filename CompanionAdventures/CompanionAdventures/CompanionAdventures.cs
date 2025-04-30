using CompanionAdventures.Framework;
using CompanionAdventures.Framework.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
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

            Store store = new Store(this);
            Test test2 = store.UseTest();
            
            // Hook events
            IModEvents events = helper.Events;
            events.GameLoop.GameLaunched += OnGameLaunched;
            events.GameLoop.UpdateTicking += OnUpdateTicking;
            events.Input.ButtonPressed += OnButtonPressed;
            events.Multiplayer.ModMessageReceived += OnMessageReceived;
            events.Player.Warped += OnPlayerWarped;
        }
        
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var api = this.Helper.ModRegistry.GetApi<IContentPack>("Pathoschild.ContentPatcher");
        }

        private void OnUpdateTicking(object? sender, UpdateTickingEventArgs e)
        {
            CompanionManager.OnUpdateTicking();
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
                if (npc.CurrentDialogue.Count > 0 || !CompanionManager.IsNpcValidCompanionForFarmer(farmer, npc))
                {
                    return;
                }

                // Suppress default behavior 
                Helper.Input.Suppress(e.Button);
                
                // Is the NPC being talked to currently a companion?
                if (CompanionManager.IsCurrentlyCompanionForFarmer(farmer, npc))
                {
                    // If yes, show options for this companion
                    CompanionManager.CompanionOptions(farmer, npc);
                }
                else
                {
                    // If no, show the option to add this NPC as a companion
                    CompanionManager.CompanionAdd(farmer, npc);
                }
            }
        }
        
        private void OnMessageReceived(object? sender, ModMessageReceivedEventArgs e)
        {
            // Early Exit: The message received was from a different mod
            if (e.FromModID != ModManifest.UniqueID)
            {
                return;
            }

            switch (e.Type)
            {
                case Constants.MessagetypeCompanionAdded:
                    CompanionManager.OnCompanionAdded(e.ReadAs<CompanionData>());
                    break;
                case Constants.MessagetypeCompanionRemoved:
                    CompanionManager.OnCompanionRemoved(e.ReadAs<CompanionData>());
                    break;
                case Constants.MessagetypeCompanionUpdated:
                    CompanionManager.OnCompanionUpdated(e.ReadAs<CompanionData>());
                    break;
                default:
                {
                    string data = e.ReadAs<string>();
                    Monitor.Log($"Received unknown event type: \"{e.Type}\" event with data: \"{data}\"", LogLevel.Error);
                    break;
                }
            }
        }

        private void OnPlayerWarped(object? sender, WarpedEventArgs e)
        {
            CompanionManager.OnPlayerWarped(e.Player);
        }
    }
}