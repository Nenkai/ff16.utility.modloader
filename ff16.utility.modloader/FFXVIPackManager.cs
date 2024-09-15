using ff16.utility.modloader.Configuration;

using FF16Tools.Pack;

using Reloaded.Mod.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ff16.utility.modloader;

public class FFXVIPackManager : IFFXVIPackManager
{
    private IModConfig _modConfig;
    private IModLoader _modLoader;
    private ILogger _logger;
    private Config _configuration;

    private FF16PackManager _packManager;

    public FFXVIPackManager(IModConfig modConfig, IModLoader modLoader, ILogger logger, Config configuration)
    {
        _modConfig = modConfig;
        _modLoader = modLoader;
        _logger = logger;
        _configuration = configuration;
    }

    public bool Initialize(string dir)
    {
        try
        {
            _packManager = FF16PackManager.Open(dir);
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"Unable to open packs: {ex.Message}");
            return false;
        }

        return true;
    }

    public bool PackExists(string packName)
    {
        return _packManager.PackFiles.ContainsKey(packName);
    }

    public void Dispose()
    {
        _packManager.Dispose();
    }
}
