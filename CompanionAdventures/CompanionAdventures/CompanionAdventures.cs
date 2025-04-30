using CompanionAdventures.Framework;
using StardewModdingAPI;

namespace CompanionAdventures
{
    public class CompanionAdventures : Mod
    {
        private static CompanionAdventures _instance = null!;
        public ModConfig Config = null!;
        
        /// <summary>
        /// The mod entry point, called after the mod is first loaded.
        /// </summary>
        public override void Entry(IModHelper helper)
        {
            // This class is set up to follow a singleton/store pattern.
            // SMAPI will instantiate this class so we don't need to worry about manually instantiating this instance
            // if it doesn't exist. So we just set the internal _instance to be this.
            _instance = this;
            
            // Initialize state
            this.Config = Helper.ReadConfig<ModConfig>();
            
            // Register events
            Events.RegisterEvents();
        }
        
        /// <summary>
        /// Get a reference to this store
        /// </summary>
        /// <returns>An instance of the CompanionAdventures store</returns>
        public static CompanionAdventures UseCompanionAdventures()
        {
            return _instance;
        }
    }
}