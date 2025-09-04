using CompanionFramework.Framework;
using CompanionFramework.Framework.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Constants = CompanionFramework.Framework.Constants;

namespace CompanionFramework
{
    public class CompanionFramework : Mod
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
            this.Helper.Events.Input.ButtonPressed += OnButtonPressed;
            this.Helper.Events.GameLoop.DayStarted += OnDayStarted;
            this.Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        }
        
        /// <summary>
        /// Handles "OnButtonPressed" events for the CompanionFramework mod.
        ///
        /// More specifically will check if the button pressed was the action button, if the farmer is trying to
        /// interact with a NPC and if the NPC has interactions relating to CompanionFramework.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            // Ignore if player isn't in the world or if they are in a cutscene
            if (!Context.IsPlayerFree)
                return;

            Resources resources = UseResources();
            // Early Exit: If mod is not enabled return
            if (!resources.Enabled)
            {
                return;
            }

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
        
                // Early Exit: If this npc is not a companion then return
                if (!companions.TryGetCompanion(npc, out Companion? companion))
                {
                    resources.Monitor.Log($"{npc.Name} is not a companion");
                    return;
                }
        
                // Early Exit: Is this companion a valid companion for this farmer (check their heart level)
                if (!companion!.IsCompanionValidForFarmer(farmer))
                {
                    resources.Monitor.Log($"{farmer.Name} has not met the requirements for {npc.Name} to be a companion");
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
                else
                {
                    resources.Monitor.Log($"{npc.Name} is unavailable to be a companion today");
                }
            }
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            Resources resources = UseResources();
            
            // Early Exit: If mod is not enabled return
            if (!resources.Enabled)
            {
                return;
            }
            
            resources.Monitor.Log($"Preparing companions for new day!");
            
            Companions companions = UseCompanions();
            companions.OnDayStarted();
        }
        
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            Resources resources = UseResources();
            
            // Early Exit: If we are the host or playing singleplayer enable the mod
            if (Context.IsMainPlayer)
            {
                resources.Enabled = true;
                resources.Monitor.Log($"Companion Framework enabled!");
                return;
            }
            
            // If we are not the host and playing multiplayer check if the host has the same mod and version
            ISemanticVersion? hostVersion = this.Helper.Multiplayer.GetConnectedPlayer(Game1.MasterPlayer.UniqueMultiplayerID)?.GetMod(this.ModManifest.UniqueID)?.Version;
            if (hostVersion == null)
            {
                resources.Enabled = false;
                resources.Monitor.Log("Companion Framework disabled because the host player doesn't have it installed.", LogLevel.Warn);
            }
            else if (hostVersion.IsOlderThan(Constants.MinHostVersion))
            {
                resources.Enabled = false;
                resources.Monitor.Log($"Companion Framework disabled because the host player has {this.ModManifest.Name} {hostVersion}, but the minimum compatible version is {Constants.MinHostVersion}.", LogLevel.Warn);
            }
            else
            {
                resources.Enabled = true;
                resources.Monitor.Log($"Companion Framework enabled!");
            }
        }
    }
}