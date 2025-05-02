using CompanionAdventures.Framework;
using StardewModdingAPI;

namespace CompanionAdventures
{
    public class CompanionAdventures : Mod
    {
        /// <summary>
        /// The mod entry point, called after the mod is first loaded.
        /// </summary>
        public override void Entry(IModHelper helper)
        {
            // Create a new store instance, this will hold all the data for this instance of the mod
            Store store = new Store(this);
            
            // Hook events
            Events events = store.UseEvents();
            events.RegisterEvents();
        }
    }
}