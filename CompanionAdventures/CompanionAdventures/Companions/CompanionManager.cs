using CompanionAdventures.Multiplayer;
using StardewModdingAPI;
using StardewValley;

#nullable disable
namespace CompanionAdventures.Companions;

public class CompanionManager
{
    private static CompanionManager Instance;
    
    private readonly IMonitor Monitor;
    private readonly MultiplayerManager MultiplayerManager;
    
    public Dictionary<Farmer, List<NPC>> CurrentCompanions = new();
    public int MaxCompanions = 3;
    public int CompanionHeartsThreshold = 5;

    public List<string> ValidCompanions = new List<string> {"Abigail", "Penny"};
    
    
    private CompanionManager(CompanionAdventures mod, IModHelper helper)
    {
        Monitor = mod.Monitor;
        MultiplayerManager = MultiplayerManager.New(mod, helper);
    }
    
    public static CompanionManager New(CompanionAdventures mod, IModHelper helper)
    {
        Instance ??= new CompanionManager(mod, helper);

        return Instance;
    }
    
    // Makes npc start following player

    /// <summary>
    /// Attempts to add the npc to the farmers companion list
    /// </summary>
    /// <returns>
    /// Returns true if the companion was added or false if it couldn't be added.
    /// </returns>
    public bool AddCompanion(Farmer farmer, NPC npc)
    {
        Monitor.Log($"Attempting to add {npc.Name} as a companion to {farmer.Name}.", LogLevel.Trace);
        
        // Check if NPC is already a companion
        if (IsCompanion(npc))
        {
            Monitor.Log($"Could not add {npc.Name} as a companion to {farmer.Name}. {npc.Name} is already a companion!", LogLevel.Trace);
            return false;
        }
        
        // Get farmers list of companions if they have one
        if (CurrentCompanions.TryGetValue(farmer, out List<NPC> companions))
        {
            // If farmer has more than or equal to the maximum number of companions
            if (companions.Count >= MaxCompanions)
            {
                Monitor.Log(
                    $"Could not add {npc.Name} as a companion to {farmer.Name}. {farmer.Name} already has the maximum number of companions!",
                    LogLevel.Trace
                );
                return false;
            }

            // If farmer doesn't have the ore than or equal to the maximum number of companions, add this companion
            companions.Add(npc);
            Monitor.Log($"Successfully added {npc.Name} as a companion to {farmer.Name}.", LogLevel.Trace);
            MultiplayerManager.SendMessage($"Successfully added {npc.Name} as a companion to {farmer.Name}");
            return true;
        }
        // Farmer doesn't exist in dictionary, add them and create a new list
        // Because farmer doesn't exist this means that they must have 0 companions, and it should be safe to add this
        // companion
        else
        {
            CurrentCompanions.Add(farmer, new List<NPC> { npc });
            Monitor.Log($"Successfully added {npc.Name} as a companion to {farmer.Name}.", LogLevel.Trace);
            MultiplayerManager.SendMessage($"Successfully added {npc.Name} as a companion to {farmer.Name}");
            return true;
        }
    }

    public bool IsNPCValidCompanion(NPC npc)
    {
        Monitor.Log($"Checking if {npc.Name} can be a valid companion.", LogLevel.Trace);

        if (ValidCompanions.Contains(npc.Name))
        {
            Monitor.Log($"{npc.Name} can be a companion.", LogLevel.Trace);
            return true;
        }
        
        Monitor.Log($"{npc.Name} can not be a companion.", LogLevel.Trace);
        return false;
    }

    public bool IsNPCValidCompanionForFarmer(Farmer farmer, NPC npc)
    {
        Monitor.Log($"Checking if {npc.Name} can be a valid companion for {farmer.Name}.", LogLevel.Trace);
        // Early Exit: If NPC can't be a companion return
        if (!IsNPCValidCompanion(npc))
            return false;
        
        // Get the heart level of the farmer and this npc
        var hearts = Util.GetHeartLevel(farmer, npc);

        // Return true if number of hearts is equal to or above heart threshold
        if (hearts >= CompanionHeartsThreshold)
        {
            Monitor.Log($"{npc.Name} can be a valid companion for {farmer.Name}.", LogLevel.Trace);
            return true;
        }
        
        Monitor.Log($"{npc.Name} is not a valid companion for {farmer.Name}.", LogLevel.Trace);
        return false;
    }
    
    public List<NPC> GetCurrentCompanions(Farmer player)
    {
        return CurrentCompanions.GetValueOrDefault(player);
    }

    public bool IsCompanion(NPC npc)
    {
        foreach (var entry in CurrentCompanions)
        {
            if (entry.Value.Contains(npc))
                return true;
        }

        return false;
    }

    public bool IsCurrentlyCompanionForFarmer(Farmer farmer, NPC npc)
    {
        // Try to get companions for this farmer
        if (CurrentCompanions.TryGetValue(farmer, out List<NPC> companions))
        {
            // Check if list of companions contains NPC
            return companions.Contains(npc);
        }

        // Farmer does not have any companions so this npc isn't currently a companion
        return false;
    }
}