using Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;
using Aloe.Common.AloeCoreLib.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Serilog.Core;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.Logging;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Views.Cust;

/// <summary>
/// PatientWindow.xaml の相互作用ロジック
/// </summary>
public partial class PatientWindow : Window
{
    private readonly ILogger _logger;
    private readonly ReservationEquipViewModel _vm;

    public PatientWindow(
        ILogger<PatientWindow> logger,
        ReservationEquipViewModel vm)
    {
        this.InitializeComponent();
        this.Closed += App.Current.Window_OnClosed;


        this._logger = logger;
        this._vm = vm;
        this.DataContext = vm;
    }

    protected override async void OnContentRendered(EventArgs e)
    {
        try
        {
            base.OnContentRendered(e);

            this.BeginInit();
            this._vm.FunctionBarVm.SharedCanExecute.Value = false;

            // 準備で、設備をロードしておく
            await this._vm.Preload();

            // 初回自動実行(検索)
            await this._vm.SearchAsync();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, ex.ToString());
        }
        finally
        {
            this._vm.FunctionBarVm.SharedCanExecute.Value = true;
            this.EndInit();
        }
    }
}
