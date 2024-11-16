using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AloeReservationGrid.App.ReservationApp;
internal static class WindowExtensions
{
    public static void ActivateOrShow(this Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

        // UIスレッド以外からの呼び出しを考慮
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (window.Visibility == Visibility.Visible)
            {
                window.Activate();
            }
            else
            {
                window.Show();
            }
        });
    }
}
