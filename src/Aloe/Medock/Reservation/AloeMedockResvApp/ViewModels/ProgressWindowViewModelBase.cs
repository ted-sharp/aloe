using Aloe.Common.AloeCoreLib.Client.Mvvm;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;

public class ProgressWindowViewModelBase : ViewModelBase, INotifyPropertyChanged, IDisposable
{
    public required InformationBarViewModel InformationBarVm { get; set; }

    public async Task RunWithProgressAsync(
        Func<Task> taskFunc,
        string startStatus = "loading...",
        string stopStatus = "done.")
    {
        try
        {
            this.InformationBarVm.StartProgress(startStatus);

            await taskFunc();
        }
        finally
        {
            this.InformationBarVm.StopProgress(stopStatus);
        }
    }
}
