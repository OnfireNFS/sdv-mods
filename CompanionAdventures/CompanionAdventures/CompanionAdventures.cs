using CompanionAdventures.Companions;
using CompanionAdventures.Events;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

#nullable disable
namespace CompanionAdventures
{
    public class CompanionAdventures : Mod
    {
        public EventManager EventManager;
        public CompanionManager CompanionManager;
        
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            
            EventManager = EventManager.New(this, helper);
            CompanionManager = CompanionManager.New(this, helper);
            
            EventManager.RegisterEvents(helper.Events);
        }
        
        /*********
         ** Private methods
         *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // Ignore if player isn't in the world or if they are in a cutscene
            if (!Context.IsPlayerFree)
                return;

            // Check if the button pressed is the interact button
            if (e.Button.IsActionButton())
            {
                // https://github.com/spacechase0/StardewValleyMods/blob/develop/AdvancedSocialMenu/Mod.cs#L72-88
                
                // Create a rectangle where the cursor is to scan for NPCs
                Rectangle tileRect = new Rectangle((int)e.Cursor.GrabTile.X * 64, (int)e.Cursor.GrabTile.Y * 64, 64, 64);
                
                NPC npc = null;
                // Get the first non-monster npc inside the rectangle
                foreach (var character in Game1.currentLocation.characters)
                {
                    if (!character.IsMonster && character.GetBoundingBox().Intersects(tileRect))
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
                if (npc.CurrentDialogue.Count > 0 || !CompanionManager.IsNPCValidCompanionForFarmer(farmer, npc))
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
    }
}