using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Common.AloeCoreLib.Util;

public class PathHelper
{
    public static string FromBase(string path)
    {
        var combined = Path.IsPathRooted(path)
            ? path
            : Path.Combine(AppContext.BaseDirectory, path);

        return Path.GetFullPath(combined);
    }
}
