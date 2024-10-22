using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FF16Tools.Pack;

namespace ff16.utility.modloader.Interfaces;

public interface IFF16ModPackManager
{
    /// <summary>
    /// Whether the mod pack manager is initialized.
    /// </summary>
    public bool Initialized { get; }

    /// <summary>
    /// Underlying pack manager.
    /// </summary>
    public FF16PackManager PackManager { get; }

    /// <summary>
    /// Whether the current game is FF16 Demo.
    /// </summary>
    public bool IsDemo { get; }

    /// <summary>
    /// Folder to use for temp files.
    /// </summary>
    public string TempFolder { get; }

    /// <summary>
    /// Data directory containing packs.
    /// </summary>
    public string DataDirectory { get; }

    /// <summary>
    /// List of all modded files, which will be applied when the mod loader has loaded all mods.
    /// </summary>
    public IReadOnlyDictionary<string, IFF16ModFile> ModdedFiles { get; }

    /// <summary>
    /// Initializes the mod pack manager.
    /// </summary>
    /// <param name="dataDir">Game directory containing pack files.</param>
    /// <param name="tempFolder">Temp folder to use.</param>
    /// <param name="isDemo">Whether the game is demo.</param>
    /// <returns></returns>
    public bool Initialize(string dataDir, string tempFolder, bool isDemo = false);

    /// <summary>
    /// Registers a mod directory and its contents. The files will be applied when the mod loader has loaded all mods.
    /// </summary>
    /// <param name="modId"></param>
    /// <param name="modDir"></param>
    public void RegisterModDirectory(string modId, string modDir);

    /// <summary>
    /// Adds a new mod file. The files will be applied when the mod loader has loaded all mods.
    /// </summary>
    /// <param name="modId">Mod Id.</param>
    /// <param name="baseDir">Mod contents base directory (normally this points to the mod's FFXVI/data).</param>
    /// <param name="localPath">Local path to the file. If it starts with a pack name (relative to baseDir), it will determine the pack name.
    /// <br>Examples:</br>
    /// <code>
    /// "baseDir/0007/nxd/gamemap.nxd" -> Pack 0007, path = nxd/gamemap.nxd
    /// "baseDir/nxd/en/ui.nxd" -> Pack 0007.en, path = nxd/ui.nxd
    /// "baseDir/system/graphics/atmosphere/texture/endof/tsinglemie_atms.tex" -> Pack 0028, path = system/graphics/atmosphere/texture/endof/tsinglemie_atms.tex
    /// </code>
    /// </param>
    public void AddModdedFile(string modId, string baseDir, string localPath);

    /// <summary>
    /// Removes a modded file.
    /// </summary>
    /// <param name="gamePath">Game path.</param>
    /// <returns>Whether the file was found and removed.</returns>
    public bool RemoveModdedFile(string gamePath);
}
