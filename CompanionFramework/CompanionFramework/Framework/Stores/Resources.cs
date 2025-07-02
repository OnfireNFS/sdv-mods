global using static CompanionFramework.Framework.Resources;
using CompanionFramework.Framework.Models;
using StardewModdingAPI;

namespace CompanionFramework.Framework;

public class ResourceConfig
{
    public ModConfig? Config { get; private init; }
    public IModHelper? Helper { get; private init; }
    public IManifest? Manifest { get; private init; }
    public IMonitor? Monitor { get; private init; }

    public class ResourceConfigBuilder
    {
        private ModConfig? _config { get; set; }
        private IModHelper? _helper { get; set; }
        private IManifest? _manifest { get; set; }
        private IMonitor? _monitor { get; set; }

        public ResourceConfigBuilder Config(ModConfig config)
        {
            this._config = config;
            return this;
        }

        public ResourceConfigBuilder Helper(IModHelper helper)
        {
            this._helper = helper;
            return this;
        }

        public ResourceConfigBuilder Manifest(IManifest manifest)
        {
            this._manifest = manifest;
            return this;
        }

        public ResourceConfigBuilder Monitor(IMonitor monitor)
        {
            this._monitor = monitor;
            return this;
        }
        
        public ResourceConfig Build()
        {
            return new ResourceConfig
            {
                Config = this._config,
                Helper = this._helper,
                Manifest = this._manifest,
                Monitor = this._monitor,
            };
        }
    }

    public static ResourceConfigBuilder Builder()
    {
        return new ResourceConfigBuilder();
    }
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

    public bool Enabled { get; set; } = false;

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