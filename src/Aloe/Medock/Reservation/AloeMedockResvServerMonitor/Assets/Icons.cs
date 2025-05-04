using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Aloe.Common.AloeCoreLib.Win32;

namespace Aloe.Medock.Reservation.AloeMedockResvServerMonitor.Assets;

public static class Icons
{
    public static Lazy<Icon> PlayCircle = new(() => Images.PlayCircle.Value.ToIcon());
    public static Lazy<Icon> StopCircle = new(() => Images.StopCircle.Value.ToIcon());
    public static Lazy<Icon> Hourglass = new(() => Images.Hourglass.Value.ToIcon());
    public static Lazy<Icon> PauseCircle = new(() => Images.PauseCircle.Value.ToIcon());
    public static Lazy<Icon> Cancel = new(() => Images.Cancel.Value.ToIcon());
    public static Lazy<Icon> Circle = new(() => Images.Circle.Value.ToIcon());
}
