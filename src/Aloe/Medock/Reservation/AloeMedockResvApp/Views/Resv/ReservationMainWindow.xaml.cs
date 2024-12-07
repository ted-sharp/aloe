using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Aloe.Medock.Reservation.AloeMedockResvApp.Utils;
using Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Login;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;
using Grpc.Net.Client;
using MagicOnion.Client;
using Microsoft.Extensions.Options;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Views.Resv;

/// <summary>
/// 予約のメイン画面です。
/// 指定日から1ヶ月間の予約件数を表示できます。
/// </summary>
public partial class ReservationMainWindow : Window
{
    private readonly ReservationMainViewModel _vm;

    private readonly ScrollViewerSynchronizer _synchronizer = new();

    public ReservationMainWindow(ReservationMainViewModel vm)
    {
        this.InitializeComponent();

        this._vm = vm;

        // StartDate を変更したときに画面のヘッダーを作り直す処理を登録
        this._vm.RefreshAction = this.RefreshScheduleHeaders;

        this.DataContext = vm;

        this.InitializeSchedules();
    }

    private void ReservationMainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        this.InitializeScrollViewers();
    }

    private void ReservationMainWindow_OnUnloaded(object sender, RoutedEventArgs e)
    {
        this._synchronizer.Clear();
    }

    /// <summary>
    /// スクロールバーの同期イベントを登録します。
    /// </summary>
    private void InitializeScrollViewers()
    {
        this._synchronizer.AddScrollViewer(this.ScrollViewer0);
        this._synchronizer.AddScrollViewer(this.ScrollViewer1);
        this._synchronizer.AddScrollViewer(this.ScrollViewer2);
    }

    /// <summary>
    /// 画面のコンポーネントを動的に生成します。
    /// DataGrid側で複雑なヘッダーは構築できないのでGridでヘッダー部分を作成しています。
    /// Gridでヘッダーの日付曜日部分を、DataGridでAM/PMのカラムを作成します。
    /// </summary>
    /// <remarks>
    /// View側の処理なのでViewModelにはしません。
    /// </remarks>
    private void InitializeSchedules()
    {
        // DataGrid の ItemsSource はバインドしますので、初回の作成が必要です
        this.GenerateDataGridColumns("G0", this.BodyDataGrid0);
        this.GenerateDataGridColumns("G1", this.BodyDataGrid1);
        this.GenerateDataGridColumns("G2", this.BodyDataGrid2);

        // StartDate の変更がトリガですが、初回は明示的に呼び出します
        var startDate = this._vm.StartDate.Value.ToDateOrToday();
        this.RefreshScheduleHeaders(startDate);
    }

    /// <summary>
    /// 日付および曜日のヘッダー部分を作成します。
    /// ViewModel側からも呼ぶため、DispatcherでUIスレッドで実行しています。
    /// </summary>
    private void RefreshScheduleHeaders(DateTime startDate)
    {
        if (Application.Current.Dispatcher.CheckAccess())
        {
            // UIスレッド上で直接処理
            RefreshScheduleHeadersInternal();
        }
        else
        {
            // UIスレッド外ならInvokeで処理を移譲
            Application.Current.Dispatcher.Invoke(RefreshScheduleHeadersInternal);
        }

        return;

        // local function
        void RefreshScheduleHeadersInternal()
        {
            this.GenerateHeaderGrid("G0", this.HeaderGrid0, startDate);
            this.GenerateHeaderGrid("G1", this.HeaderGrid1, startDate);
            this.GenerateHeaderGrid("G2", this.HeaderGrid2, startDate);
        }
    }

    #region DataGrid

    private void GenerateDataGridColumns(string gridName, DataGrid dataGrid)
    {
        try
        {
            dataGrid.BeginInit();

            dataGrid.Columns.Clear();
            dataGrid.Columns.Add(CreateDataGridColumn($"{gridName}_Room", "Room"));

            var max = this._vm.OffsetDayCount.Value;
            for (var i = 0; i < max; i++)
            {
                dataGrid.Columns.Add(CreateDataGridColumn($"{gridName}_C{i}AM", "AM"));
                dataGrid.Columns.Add(CreateDataGridColumn($"{gridName}_C{i}PM", "PM"));
            }
        }
        finally
        {
            dataGrid.EndInit();
        }

        return;

        // local function
        DataGridTextColumn CreateDataGridColumn(string columnName, string header)
        {
            var col = new DataGridTextColumn { Header = header };
            this.RegisterName(columnName, col);
            return col;
        }
    }

    #endregion DataGrid

    #region Grid

    private void GenerateHeaderGrid(string gridName, Grid grid, DateTime startDate)
    {
        try
        {
            grid.BeginInit();

            var max = this._vm.OffsetDayCount.Value;

            grid.Children.Clear();
            grid.Children.Capacity = max * 2;

            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();

            // 日付用
            grid.RowDefinitions.Add(new RowDefinition());
            // 曜日用
            grid.RowDefinitions.Add(new RowDefinition());

            grid.ColumnDefinitions.Add(CreateGridColumnDefinition($"{gridName}_Room"));

            for (var i = 0; i < max; i++)
            {
                grid.ColumnDefinitions.Add(CreateGridColumnDefinition($"{gridName}_C{i}AM"));
                grid.ColumnDefinitions.Add(CreateGridColumnDefinition($"{gridName}_C{i}PM"));

                var date = startDate.AddDays(i);
                grid.Children.Add(CreateGridDateLabel(date, i));
                grid.Children.Add(CreateGridDowLabel(date, i));
            }

            // 末尾の余白用
            grid.ColumnDefinitions.Add(new ColumnDefinition());
        }
        finally
        {
            grid.EndInit();
        }

        return;

        // local function
        static ColumnDefinition CreateGridColumnDefinition(string bindingColumnName)
        {
            var col = new ColumnDefinition();
            var binding = new Binding
            {
                ElementName = bindingColumnName,
                Path = new PropertyPath("ActualWidth"),
                Mode = BindingMode.OneWay,
            };
            col.SetBinding(ColumnDefinition.WidthProperty, binding);
            return col;
        }

        // local function
        static Label CreateGridDateLabel(DateTime date, int offset)
        {
            var lbl = new Label
            {
                Content = date.ToString("MM/dd"),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };

            Grid.SetColumn(lbl, 1 + offset * 2);
            Grid.SetColumnSpan(lbl, 2);

            return lbl;
        }

        // local function
        static Label CreateGridDowLabel(DateTime date, int offset)
        {
            var lbl = new Label
            {
                Content = date.ToString("ddd"),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };

            Grid.SetRow(lbl, 1);
            Grid.SetColumn(lbl, 1 + offset * 2);
            Grid.SetColumnSpan(lbl, 2);

            return lbl;
        }
    }

    #endregion Grid

}
