using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace CompanionAdventures
{
    
    internal sealed class CompanionAdventures : Mod
    {
        internal const String LibraryName = "libcompanionadventures";
        
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            try
            {
                var result = Native.loaded();
                Monitor.Log($"Native code loaded: {result}", LogLevel.Debug);
            }
            catch (DllNotFoundException e)
            {
                Monitor.Log($"Error loading native library '{LibraryName}': {e.Message}", LogLevel.Error);
                Monitor.Log("Ensure the correct native library for the current platform and architecture is deployed.");
            }
            catch (EntryPointNotFoundException e)
            {
                Monitor.Log($"Error finding entry point in native library '{LibraryName}': {e.Message}", LogLevel.Error);
            }
            
            // var result1 = add(1, 2);
            // this.Monitor.Log($"Message from Rust: Add 1 + 2: {result1}.", LogLevel.Debug);
            // var result2 = subtract(8, 3);
            // this.Monitor.Log($"Message from Rust: Subtract 8 - 3: {result2}.", LogLevel.Debug);
            
            Monitor.Log(Constants.TargetPlatform.ToString(), LogLevel.Debug);
            
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
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
            this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);
        }
    }
}