using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvApp.Services.CacheServices;
using Aloe.Medock.Reservation.AloeMedockResvApp.Utils;
using Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;
using Microsoft.Extensions.Logging;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Views.Resv;

public partial class ReservationDailyGrid : UserControl
{
    private readonly ILogger _logger;
    //private readonly ReservationRoomCacheService _cache;
    //private ReservationRoomTabItemViewModel? _vm;

    private bool _isLoadedRectangle = false;
    private static readonly int s_cellWidth = 88;
    private static readonly int s_cellHeight = 22;
    private static readonly int s_cellBorderThickness = 1;


    // 左上 Row=0, Col=0, Text(日付)
    // カラムヘッダー Row=0, Col, Text(ルーム)
    // ローヘッダー Row, Col=0, Text(スロット)
    // 内容 Row, Col, RowSpan, Text(x/y), SelectionAdorner



    public static bool IsInDesignMode =>
        (bool)DesignerProperties.IsInDesignModeProperty
            .GetMetadata(typeof(DependencyObject)).DefaultValue;

    public ReservationDailyGrid()
    {
        this.InitializeComponent();

        if (ReservationDailyGrid.IsInDesignMode)
        {
            // デザイナーでエラーになるので回避
            this._logger = null!;
            //this._cache = null!;

            return;
        }

        this._logger = App.Resolve<ILogger<ReservationDailyGrid>>();
        //this._cache = App.Resolve<ReservationRoommentCacheService>();



        // TODO: とりあえずダミーで設定しておく
        this.RefreshRectangle(20, 16);
    }

    /// <summary>
    /// 親要素から DataContext が設定されたときに追加で関連付けます。
    /// </summary>
    private void ReservationDailyGrid_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        // TODO: ヘッダーはまず変わらないので、先に作ってしまう

        if (this._isLoadedRectangle)
        {
            this._isLoadedRectangle = true;

            // TODO: DataContext から数を取得
            this.RefreshRectangle(20, 16);
        }

        //this._vm = this.DataContext as ReservationRoomTabItemViewModel;
        //if (this._vm != null)
        //{
        //    this._vm.RefreshFuncAsync = this.RefreshAsync;
        //}
    }

    private void RefreshRectangle(int rows, int columns)
    {
        // 行・列定義
        for (var i = 0; i < columns; i++)
        {
            this.BookingGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(ReservationDailyGrid.s_cellWidth) });
        }
        for (var i = 0; i < rows; i++)
        {
            this.BookingGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(ReservationDailyGrid.s_cellHeight) });
        }

        // 横線
        for (var row = 1; row < rows; row++)
        {
            var line = new Rectangle
            {
                Height = s_cellBorderThickness,
                Fill = Brushes.Gray,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            Grid.SetRow(line, row);
            Grid.SetColumnSpan(line, columns);
            this.BookingGrid.Children.Add(line);
        }

        // 縦線
        for (var col = 1; col < columns; col++)
        {
            var line = new Rectangle
            {
                Width = s_cellBorderThickness,
                Fill = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            Grid.SetColumn(line, col);
            Grid.SetRowSpan(line, rows);
            this.BookingGrid.Children.Add(line);
        }

        // TODO: とりあえずダミーをいれておく
        // セルごとの仮コンテンツ（例：TextBlock）
        for (var row = 1; row < rows; row++)
        {
            for (var col = 1; col < columns; col++)
            {
                var text = new TextBlock
                {
                    Text = $"({row},{col})",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetRow(text, row);
                Grid.SetColumn(text, col);
                this.BookingGrid.Children.Add(text);
            }
        }

    }

    /// <summary>
    /// 画面のコンポーネントを動的に生成します。
    /// </summary>
    /// <remarks>
    /// View側の処理なのでViewModelにはしません。
    /// </remarks>
    public async Task RefreshAsync(DateTime monthEndDate, int equipId)
    {
        var time = new Timestamper("RefreshAsync");
        try
        {
            this.BeginInit();

            //if (this._vm == null)
            //{
            //    // 初期化前は回避
            //    return;
            //}

            //this.BookingOverflowListBox.ItemsSource = null;
            //this.BookingDataGrid.ItemsSource = null;
            ////this.BookingListView.ItemsSource = null;

            //await this._vm.LoadAsync(monthEndDate, equipId);
            ////await this._vm.LoadAsync2(monthEndDate, equipId);
            //time.Stamp("loaded");

            //this.GenerateDataGridColumns(this.BookingDataGrid, monthEndDate);
            ////this.GenerateDataGridColumns2(this.BookingDataGrid, monthEndDate);
            ////this.GenerateGridViewColumns(this.BookingGridView, monthEndDate);
            //time.Stamp("columns");

            //// Observable ではないため、設定し直します。
            //this.BookingOverflowListBox.ItemsSource = this._vm.Overflows;
            //this.BookingDataGrid.ItemsSource = this._vm.Rows;
            ////this.BookingDataGrid.ItemsSource ??= this._vm.RecyclingRows;
            ////this.BookingDataGrid.Items.Refresh();
            ////this.BookingListView.ItemsSource = this._vm.Rows;

            time.Stamp("datasource");
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, ex.ToString());
            Debug.WriteLine(ex.ToString());
        }
        finally
        {
            //this.EndInit();

            await this.Dispatcher.InvokeAsync(this.EndInit, DispatcherPriority.Background);

            // 中身を入れ替えても即時に反映されないので、強制更新
            //this.BookingOverflowListBox.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            //this.BookingOverflowListBox.UpdateLayout();

            time.Stamp("finally");
            time.DumpAsync();
        }
    }

}
