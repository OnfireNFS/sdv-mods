using CompanionAdventures.Framework.Models;
using StardewModdingAPI;

namespace CompanionAdventures.Framework;

/// <summary>
/// Store pattern that holds most of the "resources" that objects/functions in this mod can use.
/// </summary>
/// <param name="mod"></param>
public partial class Store(CompanionAdventures mod)
{
    private Dictionary<Type, StoreBase> _stores = new();

    private T DefineStore<T>() where T : StoreBase
    {
        if (_stores.TryGetValue(typeof(T), out StoreBase? store))
        {
            return (T)store;
        }
        
        T newStore = Activator.CreateInstance<T>();
        newStore.store = this;
        
        _stores.Add(typeof(T), newStore);
        
        return newStore;
    }
    
    // Helper methods that return useful things that are part of `mod`
    public ModConfig Config => mod.Config;
    public IModHelper Helper =>  mod.Helper;
    public IManifest Manifest => mod.ModManifest;
    public IMonitor Monitor => mod.Monitor;
}

public abstract class StoreBase
{
    public required Store store { set; get; }
}