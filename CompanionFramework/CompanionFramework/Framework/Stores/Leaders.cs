global using static CompanionFramework.Framework.Leaders;
using CompanionFramework.Framework.Models;
using StardewValley;

namespace CompanionFramework.Framework;

public class Leaders
{
    private static Leaders? _instance;
    private Dictionary<Farmer, Leader> _leaders;
    
    private Leaders()
    {
        _leaders = new Dictionary<Farmer, Leader>
        {

        };
    }
    
    public static Leaders UseLeaders()
    {
        return _instance ??= new Leaders();
    }

    public Leader Get(Farmer farmer)
    {
        return _instance.Get(farmer);
    }

    public void Add(Farmer farmer)
    {
        
    }

    public void Remove(Farmer farmer)
    {
        
    }
}