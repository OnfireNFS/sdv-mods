using CompanionAdventures.Framework.Models;
using StardewModdingAPI;

namespace CompanionAdventures.Framework;

/// <summary>
/// Store pattern that holds most of the "resources" that objects/functions in this mod can use.
///
/// What is a store pattern?
/// A store pattern can best be thought of as a singleton, but the difference between a singleton and a store pattern is
/// a store can be instantiated. This allows for multiple instances of a store to exist, each with a single instance of
/// their resources. Store patterns are common for managing app state in front-end frameworks such as Vue and React.
/// This store design is inspired by Pinia.
///
/// Why not just use static classes?
/// I considered this but because most of the "stores" require access to "mod" properties like Monitor and Helper which
/// aren't static it made accessing them from static classes kinda weird. It would rely on calling MyStore.SetMonitor
/// or something in a static context and there is no guarantee that function was called before attempting to access
/// Monitor. This way if the store is instantiated we know we have access to instance variables like Monitor and Helper.
/// </summary>
/// <param name="mod"></param>
public partial class Store(CompanionAdventures mod)
{
    private Dictionary<Type, IStore> _stores = new();

    private T DefineStore<T>() where T : IStore
    {
        if (_stores.TryGetValue(typeof(T), out IStore? store))
        {
            return (T)store;
        }
        
        T newStore = Activator.CreateInstance<T>();
        newStore.store = this;
        
        _stores.Add(typeof(T), newStore);
        
        return newStore;
    }
    
    // Helper methods that return useful things that are part of `mod`
    public ModConfig UseConfig()
    {
        return mod.Config;
    }

    public IModHelper UseHelper()
    {
        return mod.Helper;
    }

    public IManifest UseManifest()
    {
        return mod.ModManifest;
    }
    
    public IMonitor UseMonitor()
    {
        return mod.Monitor;
    }
}

public interface IStore
{
    Store store { set; }
}