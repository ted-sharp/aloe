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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvApp.Services.CacheServices;
using Aloe.Medock.Reservation.AloeMedockResvApp.Utils;
using Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;
using Microsoft.Extensions.Logging;
using static System.Reflection.Metadata.BlobBuilder;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Views.Resv;

public partial class ReservationEquipTabItem : UserControl
{
    private readonly ILogger _logger;
    private readonly ReservationEquipmentCacheService _cache;
    private ReservationEquipTabItemViewModel? _vm;

    public static bool IsInDesignMode =>
        (bool)DesignerProperties.IsInDesignModeProperty
            .GetMetadata(typeof(DependencyObject)).DefaultValue;

    public ReservationEquipTabItem()
    {
        this.InitializeComponent();

        if (ReservationEquipTabItem.IsInDesignMode)
        {
            // デザイナーでエラーになるので回避
            this._logger = null!;
            return;
        }

        this._logger = App.Resolve<ILogger<ReservationEquipTabItem>>();
        this._cache = App.Resolve<ReservationEquipmentCacheService>();
    }

    /// <summary>
    /// 親要素から DataContext が設定されたときに追加で関連付けます。
    /// </summary>
    private void ReservationEquipTabItem_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        this._vm = this.DataContext as ReservationEquipTabItemViewModel;
        if (this._vm != null)
        {
            this._vm.RefreshFuncAsync = this.RefreshAsync;
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
            if (this._vm == null)
            {
                // 初期化前は回避
                return;
            }

            await this._vm.LoadAsync(monthEndDate, equipId);
            time.Stamp("loaded");

            this.GenerateDataGridColumns(this.BookingDataGrid, monthEndDate);
            time.Stamp("columns");

            // Observable ではないため、設定し直します。
            this.BookingDataGrid.ItemsSource = this._vm.Rows;
            this.BookingOverflowListBox.ItemsSource = this._vm.Overflows;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, ex.ToString());
            Debug.WriteLine(ex.ToString());
        }
        finally
        {
            this.EndInit();

            // 中身を入れ替えても即時に反映されないので、強制更新
            this.BookingOverflowListBox.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            this.BookingOverflowListBox.UpdateLayout();

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
            dataGrid.BeginInit();

            dataGrid.Columns.Clear();

            for (var i = 1; i <= endDate.Day; i++)
            {
                //var column = GetOrCreateDataGridColumn(endDate.Year, endDate.Month, i);
                //this._cache.SetColumn(endDate.Year, endDate.Month, i, column);
                //dataGrid.Columns.Add(column);
                dataGrid.Columns.Add(CreateDataGridColumn(endDate.Year, endDate.Month, i));
            }
        }
        finally
        {
            dataGrid.EndInit();
        }

        return;

        // local function
        DataGridTemplateColumn GetOrCreateDataGridColumn(int year, int month, int day)
        {
            var obj = this._cache.GetColumn(year, month, day);
            if (obj is DataGridTemplateColumn column)
            {
                return column;
            }

            column = CreateDataGridColumn(year, month, day);
            this._cache.SetColumn(year, month, day, column);
            return column;
        }

        // local function
        static DataGridTemplateColumn CreateDataGridColumn(int year, int month, int day)
        {
            var date = new DateTime(year, month, day);
            //var col = new DataGridColumn
            //{
            //    Header = date.ToString("dd (ddd)"),
            //    Binding = new Binding($"DateCells[{date}]"),
            //};

            var bindingText = $"{nameof(BookingRow.DateCells)}[{date}]";
            var xaml = $$"""
                <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                    <DockPanel>
                        <TextBlock DockPanel.Dock="Left" Text="{Binding {{bindingText}}.Symbol, TargetNullValue='-', FallbackValue='-' }" HorizontalAlignment="Center" />
                        <TextBlock DockPanel.Dock="Right" Text="{Binding {{bindingText}}.Remark, TargetNullValue='-', FallbackValue='-' }" HorizontalAlignment="Center" />
                        <Border DockPanel.Dock="Bottom" BorderThickness="1,0" BorderBrush="Gray">
                            <TextBlock Text="{Binding {{bindingText}}.Name, TargetNullValue='-', FallbackValue='-' }" HorizontalAlignment="Center" />
                        </Border>
                    </DockPanel>
                </DataTemplate>
                """;
            var template = System.Windows.Markup.XamlReader.Parse(xaml) as DataTemplate;

            var col = new DataGridTemplateColumn
            {
                Header = date.ToString("MM/dd (ddd)"),
                CellTemplate = template,
            };

            return col;
        }

    }

    #endregion DataGrid
}

// Converter={StaticResource DictionaryValueConverter}, ConverterParameter=2022/01/30 0:00:00}
public class DictionaryValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Dictionary<DateTime, BookingCell> dictionary && parameter is DateTime key)
        {
            if (dictionary.TryGetValue(key, out var cellData))
            {
                return cellData.Remark;
            }
            return "Key not found";
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
