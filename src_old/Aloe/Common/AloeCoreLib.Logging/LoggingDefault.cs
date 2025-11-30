using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Common.AloeCoreLib.Logging;

public static class SerilogDefault
{
    //public static string Template = "{SourceContext} [{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} (TID: {ThreadId}){NewLine}{Exception}";
    public static string Template = "[{Timestamp:HH:mm:ss}][{Level:u3}] {Message:lj} (TID: {ThreadId}){NewLine}{Exception}";
}
