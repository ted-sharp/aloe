using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Utils;

public static class DispatcherExtensions
{
    public static void InvokeIfNeeded(this Dispatcher dispatcher, Action action)
    {
        if (dispatcher.CheckAccess())
        {
            action();
        }
        else
        {
            dispatcher.Invoke(action);
        }
    }
}
