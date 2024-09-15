using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ff16.utility.modloader;

interface IFFXVIPackManager
{
    public bool Initialize(string gameDataDir);
}
