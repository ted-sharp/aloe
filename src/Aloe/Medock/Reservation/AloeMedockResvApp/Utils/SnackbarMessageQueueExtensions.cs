using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Utils;

public static class SnackbarMessageQueueExtensions
{
    public static void ShowMessage(this SnackbarMessageQueue queue, string message)
    {
        queue.Clear();
        queue.Enqueue(message);
    }

    //public static void ShowErrorMessage(this SnackbarMessageQueue queue, string message)
    //{
    //    var errorMessage = new
    //    {
    //        Message = "エラーが発生しました！",
    //        Background = Brushes.DarkRed,
    //    };

    //    queue.Clear();
    //    queue.Enqueue(message);
    //}
}
