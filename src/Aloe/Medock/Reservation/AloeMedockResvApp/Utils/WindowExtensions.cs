using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Utils;
internal static class WindowExtensions
{
    public static void ShowOrActivate(this Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        if (Application.Current.Dispatcher.CheckAccess())
        {
            // UIスレッドでなければそのまま実行
            ActivateOrShowInternal();
        }
        else
        {
            // UIスレッドでないのでディスパッチして実行
            Application.Current.Dispatcher.Invoke(ActivateOrShowInternal);
        }

        return;

        // local function
        void ActivateOrShowInternal()
        {
            if (window.Visibility == Visibility.Visible)
            {
                window.Activate();
            }
            else
            {
                window.Show();
            }
        }
    }
}
