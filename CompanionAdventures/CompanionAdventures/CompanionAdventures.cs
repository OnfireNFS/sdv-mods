using CompanionAdventures.Framework;
using CompanionAdventures.Framework.Models;
using StardewModdingAPI;

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
            
            // Create a new store instance, this will hold all the data for this instance of the mod
            Store store = new Store(this);
            
            // Hook events
            store.Events.RegisterEvents();
        }
    }
}