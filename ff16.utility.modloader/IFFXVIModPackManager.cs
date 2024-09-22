using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ff16.utility.modloader;

interface IFF16ModPackManager
{
    public bool Initialize(string gameDataDir);
}
