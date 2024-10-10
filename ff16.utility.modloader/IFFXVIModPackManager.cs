using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ff16.utility.modloader;

interface IFF16ModPackManager
{
    /// <summary>
    /// Initializes the mod pack manager.
    /// </summary>
    /// <param name="dataDir">Game directory containing pack files.</param>
    /// <param name="tempFolder">Temp folder to use.</param>
    /// <param name="isDemo">Whether the game is demo.</param>
    /// <returns></returns>
    public bool Initialize(string dataDir, string tempFolder, bool isDemo = false);

    /// <summary>
    /// Registers a mod directory and its contents.
    /// </summary>
    /// <param name="modId"></param>
    /// <param name="modDir"></param>
    public void RegisterModDirectory(string modId, string modDir);

    /// <summary>
    /// Adds a new mod file.
    /// </summary>
    /// <param name="modId">Mod Id.</param>
    /// <param name="packDir">Mod contents base directory.</param>
    /// <param name="localPath">Local path to the file. If it starts with a pack name (relative to baseDir), it will determine the pack name.
    /// <br>Examples:</br>
    /// <code>
    /// "../0007/nxd/gamemap.nxd" -> Pack 0007, path = nxd/gamemap.nxd
    /// "../nxd/en/ui.nxd" -> Pack 0007.en, path = nxd/ui.nxd
    /// "../system/graphics/atmosphere/texture/endof/tsinglemie_atms.tex" -> Pack 0028, path = system/graphics/atmosphere/texture/endof/tsinglemie_atms.tex
    /// </code>
    /// </param>
    public void AddFile(string modId, string baseDir, string localPath);
}
