using CompanionAdventures.Framework;
using CompanionAdventures.Framework.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace CompanionAdventures
{
    public class CompanionAdventures : Mod
    {
        // Holds the configuration for this mod
        public ModConfig Config = null!;
        
        /// <summary>
        /// The mod entry point, called after the mod is first loaded.
        /// </summary>
        public override void Entry(IModHelper helper)
        {
            // Load mod config
            this.Config = helper.ReadConfig<ModConfig>();

            // Initialize resources store
            UseResources(ResourceConfig.Builder()
                .Config(this.Config)
                .Helper(this.Helper)
                .Manifest(this.ModManifest)
                .Monitor(this.Monitor)
                .Build()
            );
            
            // Hook events
            RegisterEvents();
        }
        
        /****
         ** Events
         ****/
        private void RegisterEvents()
        {
            this.Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            this.Helper.Events.Input.ButtonPressed += OnButtonPressed;
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
                this.Helper.Input.Suppress(e.Button);
                
                Companions companions = UseCompanions();
                Resources resources = UseResources();
        
                // Early Exit: If this npc is not a companion then return
                if (!companions.TryGetCompanion(npc, out Companion? companion))
                {
                    resources.Monitor.Log($"{npc.Name} is not a companion");
                    return;
                }
        
                // Early Exit: Is this companion a valid companion for this farmer (check their heart level)
                if (!companion!.IsCompanionValidForFarmer(farmer))
                {
                    return;
                }

                if (companion.IsAvailable)
                {
                    companion.AskToJoin(farmer);
                } 
                else if (companion.IsRecruited)
                {
                    if (companion.Leader == farmer)
                    {
                        companion.AskOptions();
                    }
                }
            }
        }
        
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var api = this.Helper.ModRegistry.GetApi<IContentPack>("Pathoschild.ContentPatcher");
        }
    }
}