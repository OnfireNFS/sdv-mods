using System.Runtime.InteropServices;
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
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            // print button presses to the console window
            // this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}. {Native.Version()}", LogLevel.Debug);
            this.Monitor.Log($"{Native.SayHello(Game1.player.Name)}, you pressed {e.Button}.", LogLevel.Debug);
        }
    }
}