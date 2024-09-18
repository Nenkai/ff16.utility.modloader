using ff16.utility.modloader.Configuration;
using ff16.utility.modloader.Template;

using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;

using FF16Tools.Pack.Packing;

using System.Diagnostics;
using Vortice.Win32;
using CommunityToolkit.HighPerformance;
using System.Collections.Generic;

namespace ff16.utility.modloader;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public class Mod : ModBase // <= Do not Remove.
{
    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    private readonly IModLoader _modLoader;

    /// <summary>
    /// Provides access to the Reloaded.Hooks API.
    /// </summary>
    /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
    private readonly IReloadedHooks? _hooks;

    /// <summary>
    /// Provides access to the Reloaded logger.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Entry point into the mod, instance that created this class.
    /// </summary>
    private readonly IMod _owner;

    /// <summary>
    /// Provides access to this mod's configuration.
    /// </summary>
    private Config _configuration;

    /// <summary>
    /// The configuration of the currently executing mod.
    /// </summary>
    private readonly IModConfig _modConfig;

    public FFXVIPackManager _packManager;

    private string _appDir;

    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _logger = context.Logger;
        _owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;

        _logger.WriteLine($"[{context.ModConfig.ModId}] by Nenkai", _logger.ColorBlue);
        _logger.WriteLine("- https://github.com/Nenkai", _logger.ColorBlue);
        _logger.WriteLine("- https://twitter.com/Nenkaai", _logger.ColorBlue);
        _logger.WriteLine($"[{context.ModConfig.ModId}] Initializing...");

#if DEBUG
        Debugger.Launch();
#endif

        string appLocation = _modLoader.GetAppConfig().AppLocation;
        _appDir = Path.GetDirectoryName(appLocation);

        _packManager = new FFXVIPackManager(_modConfig, _modLoader, _logger, _configuration);
        if (!_packManager.Initialize(Path.Combine(_appDir, "data")))
            _logger.WriteLine($"[{context.ModConfig.ModId}] Failed to initialize. Mods will still be attempted to be loaded.", _logger.ColorRed);

        _modLoader.AddOrReplaceController<IFFXVIPackManager>(_owner, _packManager);

        _modLoader.ModLoading += ModLoading;
        _modLoader.OnModLoaderInitialized += OnAllModsLoaded;
    }

    private Dictionary<string, ModPack> _modPackFiles = new();

    private void ModLoading(IModV1 mod, IModConfigV1 modConfig)
    {
        var modDir = Path.Combine(_modLoader.GetDirectoryForModId(modConfig.ModId), @"FFXVI\data");
        if (!Directory.Exists(modDir))
            return;

        foreach (var dir in Directory.GetDirectories(modDir))
        {
            if (Directory.Exists(dir))
            {
                string possiblePackName = Path.GetFileName(dir);
                if (_packManager.PackExists(possiblePackName))
                {
                    AddModPackFiles(modConfig.ModId, dir);
                }
            }
        }
    }

    private void AddModPackFiles(string modId, string packDir)
    {
        string packName = Path.GetFileName(packDir);
        List<string> spl = packName.Split('.').ToList();
        spl.Insert(1, "diff");
        string diffPackName = string.Join('.', spl);

        if (!_modPackFiles.TryGetValue(diffPackName, out ModPack modPack))
        {
            modPack = new ModPack();
            modPack.PackName = diffPackName;
            _modPackFiles.TryAdd(diffPackName, modPack);

            // If the pack we're adding was a localized one (i.e 0001.diff.en.pac), we need to make sure we also create
            // a diff pack for the base pack (0001.diff.pac)
            if (spl.Count > 2)
            {
                string baseDiffPack = string.Join(".", spl[0], spl[1]);

                var baseModPack = new ModPack();
                baseModPack.PackName = baseDiffPack;
                _modPackFiles.TryAdd(baseDiffPack, baseModPack);
            }

        }
        
        foreach (string file in Directory.GetFiles(packDir, "*", SearchOption.AllDirectories))
        {
            string packFilePath = Path.GetRelativePath(packDir, file);
            if (!modPack.Files.TryGetValue(file, out ModFile modFile))
            {
                modFile = new ModFile()
                {
                    ModIdOwner = modId,
                    LocalPath = file,
                    PackPath = packFilePath,
                };
                modPack.Files.Add(packFilePath, modFile);
            }
            else
            {
                // overriding
                _logger.WriteLine($"[{_modConfig.ModId}] Conflict: {modFile.PackPath} is used by {modFile.ModIdOwner}, overwritten by {modId}", _logger.ColorYellow);
                modFile.ModIdOwner = modId;
                modFile.LocalPath = file;
            }
        }
    }

    private async void OnAllModsLoaded()
    {
        // Free the pack handles.
        _packManager.Dispose();

        string dataDir = Path.Combine(_appDir, "data");
        Directory.CreateDirectory(dataDir);

        // Clean up state
        foreach (var file in Directory.GetFiles(dataDir))
        {
            if (file.Contains(".diff."))
                File.Delete(file);
        }

        foreach (ModPack pack in  _modPackFiles.Values)
        {
            var builder = new FF16PackBuilder();

            if (pack.Files.TryGetValue(".path", out ModFile pathFile))
                builder.RegisterPathFile(pathFile.LocalPath);

            foreach (ModFile file in pack.Files.Values)
            {
                if (file.PackPath.Contains(".path"))
                    continue;

                _logger.WriteLine($"[{_modConfig.ModId}] {file.ModIdOwner}: Adding file '{file.PackPath}'");
                builder.AddFile(file.LocalPath, file.PackPath);
            }

            _logger.WriteLine($"[{_modConfig.ModId}] Writing '{pack.PackName}' ({pack.Files.Count} files)...");
            await builder.WriteToAsync(Path.Combine(dataDir, $"{pack.PackName}.pac"));
        }

        _logger.WriteLine($"[{_modConfig.ModId}] FFXVI Mod loader initialized with {_modPackFiles.Count} pack(s).", _logger.ColorGreen);
    }

    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        _configuration = configuration;
        _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
    }
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}

public class ModFile
{
    public string ModIdOwner { get; set; }
    public string PackPath { get; set; }
    public string LocalPath { get; set; }
}

public class ModPack
{
    public string PackName { get; set; }
    public Dictionary<string, ModFile> Files { get; set; } = new();
}