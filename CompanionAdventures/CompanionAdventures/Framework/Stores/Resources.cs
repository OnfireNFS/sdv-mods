using CompanionAdventures.Framework.Models;
using StardewModdingAPI;

namespace CompanionAdventures.Framework;

public class ResourceConfig
{
    public ModConfig? Config { get; init; }
    public IModHelper? Helper { get; init; }
    public IManifest? Manifest { get; init; }
    public IMonitor? Monitor { get; init; }
}

/// <summary>
/// Holds references to application utilities. This is useful because they can be accessed from a static or instance
/// context without having to pass them as parameters.
/// </summary>
public class Resources
{
    private static Resources? _instance;
    private ModConfig? _config = null;
    private IModHelper? _helper = null;
    private IManifest? _manifest = null;
    private IMonitor? _monitor = null;

    public ModConfig Config
    {
        get => this._config ?? throw new NullReferenceException("App.Config");
        private set => this._config = value;
    }

    public IModHelper Helper
    {
        get => this._helper ?? throw new NullReferenceException("App.Helper");
        private set => this._helper = value;
    }

    public IManifest Manifest
    {
        get => this._manifest ?? throw new NullReferenceException("App.Manifest");
        private set => this._manifest = value;
    }
    public IMonitor Monitor
    {
        get => this._monitor ?? throw new PropertyNullException("App.Monitor");
        private set => this._monitor = value;
    }
    
    private Resources() { }

    public static Resources UseResources(ResourceConfig? config = null) {
        // Set _instance to a new instance of Utility if it is not already
        _instance ??= new Resources();

        if (config?.Config != null)
        {
            _instance.Config = config.Config;
        }
        if (config?.Helper != null)
        {
            _instance.Helper = config.Helper;
        }
        if (config?.Manifest != null)
        {
            _instance.Manifest = config.Manifest;
        }
        if (config?.Monitor != null)
        {
            _instance.Monitor = config.Monitor;
        }
        
        return _instance;
    }
    
    /****
     ** Setters
     ****/
    public void SetConfig(ModConfig config)
    {
        this.Config = config;
    }
    public void SetHelper(IModHelper helper)
    {
        this.Helper = helper;
    }
    public void SetManifest(IManifest manifest)
    {
        this.Manifest = manifest;
    }
    public void SetMonitor(IMonitor monitor)
    {
        this.Monitor = monitor;
    }
}