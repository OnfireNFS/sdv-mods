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
/// Why?
/// Originally I thought for split-screen I would need a separate companions store for each player, this seems to not be
/// the case, the store may be removed in the future, replaced with singletons or separated into classes that can be
/// manually instantiated. For example `store.UseCompanions() -> new CompanionManager(args)`. This might make
/// maintaining this mod easier as C# code generally uses Object-Oriented Programming (OOP) and Manager classes.
/// 
/// Currently, I have kept the store pattern because it is really nice when prototyping to call `store.UseThing()` from
/// basically anywhere in the mod and retrieve an instance of what I'm looking for without having to pass it through
/// args. I also don't have to worry about static vs instance methods since everything is instantiated.
/// </summary>
/// <param name="mod"></param>
public partial class Store(CompanionAdventures mod)
{
    // returns the config for this instance of mod
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