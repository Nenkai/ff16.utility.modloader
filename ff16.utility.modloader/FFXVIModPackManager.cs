using ff16.utility.modloader.Configuration;

using FF16Tools.Pack;

using Microsoft.Extensions.Logging;

using Reloaded.Mod.Interfaces;


namespace ff16.utility.modloader;

public class FF16ModPackManager : IFF16ModPackManager
{
    private IModConfig _modConfig;
    private IModLoader _modLoader;
    private Reloaded.Mod.Interfaces.ILogger _reloadedLogger;
    private Config _configuration;

    private FF16PackManager _packManager;

    private bool _initialized = false;
    private ILoggerFactory _loggerFactory;

    public FF16ModPackManager(IModConfig modConfig, IModLoader modLoader, Reloaded.Mod.Interfaces.ILogger logger, Config configuration)
    {
        _modConfig = modConfig;
        _modLoader = modLoader;
        _reloadedLogger = logger;
        _configuration = configuration;

        _loggerFactory = LoggerFactory.Create(e => e.AddProvider(new R2LoggerToMSLoggerAdapterProvider(logger)));
    }

    public bool Initialize(string dir)
    {
        try
        {
            _packManager = new FF16PackManager(_loggerFactory);
            _packManager.Open(dir);
        }
        catch (Exception ex)
        {
            _reloadedLogger.WriteLine($"[{_modConfig.ModId}] Unable to open packs: {ex.Message}");
            return false;
        }

        return true;
    }

    public bool PackExists(string packName)
    {
        if (_packManager is null)
            throw new Exception("Pack manager was not initialized.");

        return _packManager.PackFiles.ContainsKey(packName);
    }

    public void Dispose()
    {
        _packManager?.Dispose();
    }
}
