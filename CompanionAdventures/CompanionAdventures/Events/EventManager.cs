using CompanionAdventures.Multiplayer;
using StardewModdingAPI;
using StardewModdingAPI.Events;

#nullable disable
namespace CompanionAdventures.Events;

public class EventManager
{
    private static EventManager Instance;
    
    private readonly IMonitor Monitor;
    private readonly MultiplayerManager MultiplayerManager;
    
    private EventManager(CompanionAdventures mod, IModHelper helper)
    {
        Monitor = mod.Monitor;
        MultiplayerManager = MultiplayerManager.New(mod, helper);
    }
    
    public static EventManager New(CompanionAdventures mod, IModHelper helper)
    {
        Instance ??= new EventManager(mod, helper);

        return Instance;
    }
    
    /// <summary>
    /// Register the events this mod listens to and what functions should be called to handle those events
    /// </summary>
    public void RegisterEvents(IModEvents events)
    {
        events.GameLoop.SaveLoaded += new EventHandler<SaveLoadedEventArgs>((object sender, SaveLoadedEventArgs e) => {});
        events.GameLoop.Saving += new EventHandler<SavingEventArgs>((object sender, SavingEventArgs e) => {});
        events.GameLoop.ReturnedToTitle += new EventHandler<ReturnedToTitleEventArgs>((object sender, ReturnedToTitleEventArgs e) => {});
        events.GameLoop.DayStarted += new EventHandler<DayStartedEventArgs>((object sender, DayStartedEventArgs e) => {});
        events.GameLoop.DayEnding += new EventHandler<DayEndingEventArgs>((object sender, DayEndingEventArgs e) => {});
        events.GameLoop.GameLaunched += new EventHandler<GameLaunchedEventArgs>(OnGameLaunched);
        events.GameLoop.UpdateTicked += new EventHandler<UpdateTickedEventArgs>((object sender, UpdateTickedEventArgs e) => {});
        events.Display.RenderedHud += new EventHandler<RenderedHudEventArgs>((object sender, RenderedHudEventArgs e) => {});
        events.Multiplayer.ModMessageReceived += new EventHandler<ModMessageReceivedEventArgs>(MultiplayerManager.OnMessageReceived);
    }
    
    private void OnGameLaunched(object sender, GameLaunchedEventArgs e) {
        Monitor.Log($"Game Launched!", LogLevel.Debug);
    }
}