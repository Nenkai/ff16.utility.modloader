using System.Diagnostics;

using CommunityToolkit.HighPerformance.Buffers;

using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Mod.Interfaces.Internal;

using FF16Tools.Pack;
using FF16Tools.Files.Nex;
using FF16Tools.Files.Nex.Entities;

using ff16.utility.modloader.Configuration;
using ff16.utility.modloader.Template;
using ff16.utility.modloader.Interfaces;
using FF16Tools.Files;

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

    public FF16ModPackManager _modPackManager;

    private string _appDir;
    private string _tempDir;

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
        _tempDir = Path.Combine(_modLoader.GetDirectoryForModId(_modConfig.ModId), "staging");
        bool isDemo = _modLoader.GetAppConfig().AppId == "ffxvi_demo.exe";

        ClearDiffPackState();

        _modPackManager = new FF16ModPackManager(_modConfig, _modLoader, _logger, _configuration);

        if (!_modPackManager.Initialize(Path.Combine(_appDir, "data"), _tempDir, isDemo: isDemo))
        {
            _logger.WriteLine($"[{context.ModConfig.ModId}] Pack manager failed to initialize.", _logger.ColorRed);
            return;
        }

        _modLoader.AddOrReplaceController<IFF16ModPackManager>(_owner, _modPackManager);

        _modLoader.ModLoading += ModLoading;
        _modLoader.OnModLoaderInitialized += OnAllModsLoaded;
    }

    /// <summary>
    /// Removes any left-over .diff packs in the game's data directory.
    /// </summary>
    private void ClearDiffPackState()
    {
        string dataDir = Path.Combine(_appDir, "data");
        foreach (var file in Directory.GetFiles(dataDir))
        {
            if (file.Contains(".diff."))
            {
                try
                {
                    _logger.WriteLine($"[{_modConfig.ModId}] Deleting '{Path.GetFileName(file)}' for clean state");
                    File.Delete(file);
                }
                catch (IOException ioEx)
                {
                    _logger.WriteLine($"[{_modConfig.ModId}] Attempted to delete {file} for clean state but errored (IOException) - is the game already running as another process? " +
                        $"Error: {ioEx.Message}", _logger.ColorRed);
                }
                catch (Exception ex)
                {
                    _logger.WriteLine($"[{_modConfig.ModId}] Attempted to delete {file} for clean state but errored: {ex.Message}", _logger.ColorRed);
                }
            }
        }
    }

    private void ModLoading(IModV1 mod, IModConfigV1 modConfig)
    {
        var modDir = Path.Combine(_modLoader.GetDirectoryForModId(modConfig.ModId), @"FFXVI/data");
        if (!Directory.Exists(modDir))
            return;

        _modPackManager.RegisterModDirectory(modConfig.ModId, modDir);
    }

    private void OnAllModsLoaded()
    {
        if (_configuration.AddMainMenuModInfo)
            ApplyMainMenuModInfo();

        string dataDir = Path.Combine(_appDir, "data");
        if (!Directory.Exists(dataDir))
        {
            try
            {
                Directory.CreateDirectory(dataDir);
            }
            catch (Exception ex)
            {
                _logger.WriteLine($"[{_modConfig.ModId}] data folder in game directory was missing (???), attempted to create it but errored: {ex.Message}", _logger.ColorRed);
                return;
            }
        }

        _modPackManager.Apply();

        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    public void ApplyMainMenuModInfo()
    {
        const string uiNxdPath = "nxd/ui.nxd";

        FF16PackPathUtil.TryGetPackNameForPath(uiNxdPath, out string packName, out _, _modPackManager.IsDemo);

        string tempModLoaderDataDir = Path.Combine(_tempDir, "data");
        try
        {
            NexTableLayout tableColumnLayout = TableMappingReader.ReadTableLayout("ui", new Version(1, 0, 0));
            foreach (var locale in FF16PackPathUtil.PackLocales)
            {
                using var nexFileData = _modPackManager.PackManager.GetFileDataFromPackAsync(uiNxdPath, $"{packName}.{locale}").GetAwaiter().GetResult();
                NexDataFile nexFile = new NexDataFile();
                nexFile.Read(nexFileData.Span.ToArray());

                var versionRow = nexFile.RowManager.GetRowInfo(19, 0, 0);
                var builder = new NexDataFileBuilder(tableColumnLayout);
                foreach (var row in nexFile.RowManager.GetAllRowInfos())
                {
                    List<object> cells = NexUtils.ReadRow(tableColumnLayout, nexFile.Buffer, row.RowDataOffset);
                    if (row.Key == 19)
                    {
                        var mods = _modLoader.GetActiveMods()
                            .Where(m => !m.Generic.ModId.Contains("Reloaded", StringComparison.OrdinalIgnoreCase) && m.Generic.ModId != _modConfig.ModId)
                            .OrderBy(m => m.Generic.ModName)
                            .ToList();

                        string str = (string)cells[1];
                        str += "<br><br>";
                        str += $"Reloaded-II <color=txtgray>{_modLoader.GetLoaderVersion()}</color> by <color=txtgray><i>Sewer56</i></color><br>";
                        str += $"FFXVI Mod Loader <color=txtgray>{_modConfig.ModVersion}</color> by <color=txtgray><i>{_modConfig.ModAuthor}</i></color><br>";
                        str += $"<color=main_quest>FFXVI Modding</color>: <color=txtgray>nenkai.github.io/ffxvi-modding</color><br>";
                        str += $"<color=text_red>Support:</color> <color=txtgray>ko-fi.com/nenkai</color><br>";
                        str += $"<color=txtlightblue>Twitter:</color> <color=txtgray>twitter.com/Nenkaai</color><br>";

                        str += "<br>";
                        if (mods.Count > 0)
                        {
                            str += $"<color=tip_rank2><scale=1.2><b><icon=4028> Installed Mods ({mods.Count}) <icon=4028></b></color><br>";
                            str += "<scale>"; // reset scale

                            int numModsToShow = Math.Min(mods.Count, 30);
                            int rem = mods.Count % numModsToShow;
                            for (int i = 0; i < numModsToShow; i++)
                            {
                                var mod = mods[i];
                                str += $"{mod.Generic.ModName} <color=txtgray>{mod.Generic.ModVersion}</color> by <color=txtgray><i>{mod.Generic.ModAuthor}</i></color><br>";
                            }

                            if (rem > 0)
                                str += $"...and {rem} other mods";
                        }
                        else
                        {
                            str += "No mods loaded";
                        }

                        cells[1] = str;
                    }

                    builder.AddRow(row.Key, row.Key2, row.Key3, cells);
                }

                string packPath = Path.Combine(tempModLoaderDataDir, $"{packName}.{locale}");
                string stagingNxdPath = Path.Combine(packPath, uiNxdPath);
                Directory.CreateDirectory(Path.GetDirectoryName(stagingNxdPath));

                using (var fs = new FileStream(stagingNxdPath, FileMode.Create))
                    builder.Write(fs);

                _modPackManager.AddModdedFile(_modConfig.ModId, tempModLoaderDataDir, stagingNxdPath);
            }
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Cannot apply title screen ui patch - {ex.Message}", _logger.ColorRed);
            return;
        }

        // Edited ui file. Changed the version text ui element to be much larger, and vertically aligned to bottom
        // Moved DLC banners to the left of the screen
        string modIdDir = Path.Combine(_modLoader.GetDirectoryForModId(_modConfig.ModId));
        string modDir = Path.Combine(modIdDir, "FFXVI", "data");
        string titleUib = Path.Combine(modDir, "ui", "gameflow", "title", "title01.uib");

        if (!File.Exists(titleUib))
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Cannot apply title screen ui patch - ui/gameflow/title/title01.uib is missing?", _logger.ColorRed);
            return;
        }

        _modPackManager.AddModdedFile(_modConfig.ModId, modDir, titleUib);
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

public class ModPack
{
    /// <summary>
    /// Main pack name regardless of locale, i.e "0007".
    /// </summary>
    public string MainPackName { get; set; }

    /// <summary>
    /// Pack name including locale, i.e "0007" or "0007.en".
    /// </summary>
    public string BaseLocalePackName { get; set; }

    /// <summary>
    /// Diff pack name, i.e "0007.diff" or "0007.diff.en".
    /// </summary>
    public string DiffPackName { get; set; }

    /// <summary>
    /// Modded files for this pack.
    /// </summary>
    public Dictionary<string, FF16ModFile> Files { get; set; } = new();
}