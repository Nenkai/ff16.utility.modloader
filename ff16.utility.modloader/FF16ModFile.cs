using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ff16.utility.modloader.Interfaces;

namespace ff16.utility.modloader;

public class FF16ModFile : IFF16ModFile
{
    /// <summary>
    /// Mod id that owns this modded file.
    /// </summary>
    public string ModIdOwner { get; set; }

    /// <summary>
    /// Game path.
    /// </summary>
    public string GamePath { get; set; }

    /// <summary>
    /// Local path.
    /// </summary>
    public string LocalPath { get; set; }
}
