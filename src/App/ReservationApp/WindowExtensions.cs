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
