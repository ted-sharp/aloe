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
    //private readonly ReservationRoommentCacheService _cache;
    //private ReservationRoomTabItemViewModel? _vm;

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
    }

    /// <summary>
    /// 親要素から DataContext が設定されたときに追加で関連付けます。
    /// </summary>
    private void ReservationRoomTabItem_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        //this._vm = this.DataContext as ReservationRoomTabItemViewModel;
        //if (this._vm != null)
        //{
        //    this._vm.RefreshFuncAsync = this.RefreshAsync;
        //}
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

    #region DataGrid

    /// <summary>
    /// スロットの数だけ DataGrid のヘッダーを作成します。
    /// 記号、氏名、備考がスロットの数だけ繰り返し作成します。
    /// </summary>
    private void GenerateDataGridColumns(DataGrid dataGrid, DateTime endDate)
    {
        try
        {
            //dataGrid.BeginInit();

            dataGrid.Columns.Clear();

            // TODO: 設備で作成する
            //for (var i = 1; i <= endDate.Day; i++)
            //{
            //    //var column = GetOrCreateDataGridColumn(endDate.Year, endDate.Month, i);
            //    //this._cache.SetColumn(endDate.Year, endDate.Month, i, column);
            //    //dataGrid.Columns.Add(column);
            //    dataGrid.Columns.Add(CreateDataGridColumn(endDate.Year, endDate.Month, i));
            //}
        }
        finally
        {
            //dataGrid.EndInit();
        }

        return;

        // local function
        static DataGridTemplateColumn CreateDataGridColumn(int year, int month, int day)
        {
            var date = new DateTime(year, month, day);
            var col = new DataGridTemplateColumn
            {
                Header = CreateHeader(date),
                CellTemplate = CreateCellTemplate(date),
                // 固定値を設定することでレイアウト計算の負荷を減らす
                MinWidth = 160,
                MaxWidth = 160,
                Width = 160,
            };

            return col;
        }

        // local function
        static TextBlock CreateHeader(DateTime date)
        {
            // 中央寄せのために必要
            var content = new TextBlock
            {
                Text = date.ToString("MM/dd (ddd)"),
                TextAlignment = TextAlignment.Center,
                //// 固定値を設定することでレイアウト計算の負荷を減らす
                MinWidth = 160,
                MaxWidth = 160,
                Width = 160,
                MinHeight = 160,
                MaxHeight = 160,
                Height = 160,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                // 他のスタイルを参照しないことで、Resourcesへのアクセスを減らす
                Style = null,
                OverridesDefaultStyle = false,
            };
            return content;
        }

        // local function
        static DataTemplate CreateCellTemplate(DateTime date)
        {
            var template = new DataTemplate();

            var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);

            // 固定値を設定することでレイアウト計算の負荷を減らす
            textBlockFactory.SetValue(TextBlock.MinWidthProperty, 160.0);
            textBlockFactory.SetValue(TextBlock.MaxWidthProperty, 160.0);
            textBlockFactory.SetValue(TextBlock.WidthProperty, 160.0);
            textBlockFactory.SetValue(TextBlock.MinHeightProperty, 24.0);
            textBlockFactory.SetValue(TextBlock.MaxHeightProperty, 24.0);
            textBlockFactory.SetValue(TextBlock.HeightProperty, 24.0);
            textBlockFactory.SetValue(TextBlock.MarginProperty, new Thickness(0));
            textBlockFactory.SetValue(TextBlock.PaddingProperty, new Thickness(0));

            // 他のスタイルを参照しないことで、Resourcesへのアクセスを減らす
            textBlockFactory.SetValue(TextBlock.StyleProperty, null);
            textBlockFactory.SetValue(TextBlock.OverridesDefaultStyleProperty, false);

            var bindingText = $"{nameof(BookingRow.DateCells)}[{date}].{nameof(BookingCell.DisplayText)}";
            var binding = new Binding(bindingText)
            {
                Mode = BindingMode.OneTime,
                TargetNullValue = "-",
                FallbackValue = "-",
                IsAsync = true,
            };
            textBlockFactory.SetBinding(TextBlock.TextProperty, binding);

            template.VisualTree = textBlockFactory;
            return template;

        }

        //// local function
        //static Style CreateCellStyle(DateTime date)
        //{
        //    var style = new Style(typeof(DataGridCell));

        //    // デフォルトの背景色
        //    style.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.White));

        //    // 土日の背景色変更トリガー
        //    var trigger = new DataTrigger
        //    {
        //        Binding = new Binding
        //        {
        //            Path = new PropertyPath(nameof(BookingCell.IsWeekend)),
        //            Source = new BookingCell { IsWeekend = IsWeekend(date) } // 動的プロパティを設定
        //        },
        //        Value = true
        //    };
        //    trigger.Setters.Add(new Setter(DataGridCell.BackgroundProperty, Brushes.LightCoral));

        //    style.Triggers.Add(trigger);

        //    return style;
        //}
    }



    /// <summary>
    /// スロットの数だけ DataGrid のヘッダーを作成します。
    /// 記号、氏名、備考がスロットの数だけ繰り返し作成します。
    /// </summary>
    private void GenerateDataGridColumns2(DataGrid dataGrid, DateTime endDate)
    {
        try
        {
            //dataGrid.BeginInit();

            if (dataGrid.Columns.Count < 31)
            {
                // 初回は全部作る
                dataGrid.Columns.Clear();
                for (var day = 1; day <= 31; day++)
                {
                    dataGrid.Columns.Add(CreateDataGridColumn(endDate.Year, endDate.Month, day));
                }
            }
            else
            {
                for (var day = 1; day <= 31; day++)
                {
                    var i = day - 1;
                    var col = dataGrid.Columns[i];
                    if (day <= endDate.Day)
                    {
                        var date = new DateTime(endDate.Year, endDate.Month, day);
                        col.Header = date.ToString("MM/dd (ddd)");
                        col.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        col.Header = "";
                        col.Visibility = Visibility.Hidden;
                    }
                }
            }

        }
        finally
        {
            //dataGrid.EndInit();
        }

        return;

        // local function
        static DataGridTemplateColumn CreateDataGridColumn(int year, int month, int day)
        {
            var date = new DateTime(year, month, day);
            var col = new DataGridTemplateColumn
            {
                Header = CreateHeader(date),
                CellTemplate = CreateCellTemplate(day),
            };

            return col;
        }

        // local function
        static TextBlock CreateHeader(DateTime date)
        {
            var content = new TextBlock
            {
                Text = date.ToString("MM/dd (ddd)"),
                TextAlignment = TextAlignment.Center,
                Style = null,
                OverridesDefaultStyle = false,
            };
            return content;
        }

        // local function
        static DataTemplate CreateCellTemplate(int day)
        {
            var template = new DataTemplate();

            var textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
            textBlockFactory.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            textBlockFactory.SetValue(TextBlock.StyleProperty, null);
            textBlockFactory.SetValue(TextBlock.OverridesDefaultStyleProperty, false);

            var bindingText = $"{nameof(RecyclingBookingRow.DateCells)}[{day}].{nameof(BookingCell.DisplayText)}";
            var binding = new Binding(bindingText)
            {
                Mode = BindingMode.OneTime,
                TargetNullValue = "-",
                FallbackValue = "-",
                IsAsync = true,
            };
            textBlockFactory.SetBinding(TextBlock.TextProperty, binding);

            template.VisualTree = textBlockFactory;
            return template;

        }
    }

    #endregion DataGrid


    #region ListView

    /// <summary>
    /// スロットの数だけ DataGrid のヘッダーを作成します。
    /// 記号、氏名、備考がスロットの数だけ繰り返し作成します。
    /// </summary>
    private void GenerateGridViewColumns(GridView dataGrid, DateTime endDate)
    {
        dataGrid.Columns.Clear();

        dataGrid.Columns.Add(CreateGridViewSlotColumn());

        for (var i = 1; i <= endDate.Day; i++)
        {
            //var column = GetOrCreateDataGridColumn(endDate.Year, endDate.Month, i);
            //this._cache.SetColumn(endDate.Year, endDate.Month, i, column);
            //dataGrid.Columns.Add(column);
            dataGrid.Columns.Add(CreateGridViewColumn(endDate.Year, endDate.Month, i));
        }

        return;

        // local function
        static GridViewColumn CreateGridViewSlotColumn()
        {
            var col = new GridViewColumn
            {
                Header = "Slot",
                DisplayMemberBinding = new System.Windows.Data.Binding("SlotDisplay"),
                Width = 40,
            };

            return col;
        }

        // local function
        static GridViewColumn CreateGridViewColumn(int year, int month, int day)
        {
            var date = new DateTime(year, month, day);
            var col = new GridViewColumn
            {
                Header = GenerateHeader(date),
                CellTemplate = GenerateCellTemplate(date),
                Width = 160,
            };

            return col;
        }

        // local function
        static TextBlock GenerateHeader(DateTime date)
        {
            var content = new TextBlock
            {
                Text = date.ToString("MM/dd (ddd)"),
                Width = 160,
                TextAlignment = TextAlignment.Center,
            };
            return content;
        }

        // local function
        static DataTemplate GenerateCellTemplate(DateTime date)
        {
            var xaml = $@"
    <DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                  xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
        <Border BorderBrush='Black' BorderThickness='1' HorizontalAlignment='Stretch' VerticalAlignment='Stretch'>
            <TextBlock Text='{{Binding DateCells[{date}].DisplayText}}'
                       HorizontalAlignment='Stretch'
                       VerticalAlignment='Stretch'
                       TextAlignment='Center' />
        </Border>
    </DataTemplate>";

            return (DataTemplate)XamlReader.Parse(xaml);
        }
    }

    #endregion ListView
}
