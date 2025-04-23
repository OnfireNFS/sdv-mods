using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace CompanionAdventures
{
    internal sealed class CompanionAdventures : Mod
    {
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            
            Monitor.Log($"Native code loaded: {Native.Version()}", LogLevel.Debug);
            Monitor.Log(Constants.TargetPlatform.ToString(), LogLevel.Debug);
            
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;

            this.RegisterEvents(helper.Events);
        }

        /// <summary>
        /// Register the events this mod listens to and what functions should be called to handle those events
        /// </summary>
        private void RegisterEvents(IModEvents events)
        {
            events.GameLoop.SaveLoaded += new EventHandler<SaveLoadedEventArgs>((object sender, SaveLoadedEventArgs e) => {});
            events.GameLoop.Saving += new EventHandler<SavingEventArgs>((object sender, SavingEventArgs e) => {});
            events.GameLoop.ReturnedToTitle += new EventHandler<ReturnedToTitleEventArgs>((object sender, ReturnedToTitleEventArgs e) => {});
            events.GameLoop.DayStarted += new EventHandler<DayStartedEventArgs>((object sender, DayStartedEventArgs e) => {});
            events.GameLoop.DayEnding += new EventHandler<DayEndingEventArgs>((object sender, DayEndingEventArgs e) => {});
            events.GameLoop.GameLaunched += new EventHandler<GameLaunchedEventArgs>((object sender, GameLaunchedEventArgs e) => {});
            events.GameLoop.UpdateTicked += new EventHandler<UpdateTickedEventArgs>((object sender, UpdateTickedEventArgs e) => {});
            events.Display.RenderedHud += new EventHandler<RenderedHudEventArgs>((object sender, RenderedHudEventArgs e) => {});
        }

        /*********
         ** Private methods
         *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            // Ignore if player isn't in the world or if they are in a cutscene
            if (!Context.IsPlayerFree)
                return;

            if (e.Button.IsActionButton())
            {
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

                if (npc == null)
                    return;
                
                // If NPC is currently not a companion and has a Dialogue message queued: do nothing
                // and let the dialogue message trigger
                if (npc.CurrentDialogue.Count > 0)
                    return;
                
                Monitor.Log($"{npc.CurrentDialogue.Count}", LogLevel.Debug);
                
                // Helper.Input.Suppress(e.Button);
            }

            // print button presses to the console window
            // this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}. {Native.Version()}", LogLevel.Debug);
            // this.Monitor.Log($"{Native.SayHello(Game1.player.Name)}, you pressed {e.Button}.", LogLevel.Debug);
        }
    }
}