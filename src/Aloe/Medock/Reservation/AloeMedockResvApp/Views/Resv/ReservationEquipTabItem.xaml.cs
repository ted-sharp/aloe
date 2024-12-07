using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Aloe.Medock.Reservation.AloeMedockResvApp.Utils;
using Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;
using static System.Reflection.Metadata.BlobBuilder;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Views.Resv;

public partial class ReservationEquipTabItem : UserControl
{
    private ReservationEquipTabItemViewModel? _vm;

    private DateTime? _latestDate;

    public ReservationEquipTabItem()
    {
        this.InitializeComponent();
    }

    private void ReservationEquipTabItem_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        this._vm = this.DataContext as ReservationEquipTabItemViewModel;
        if (this._vm != null)
        {
            this._vm.RefreshAction = this.Refresh;
        }
    }

    /// <summary>
    /// 画面のコンポーネントを動的に生成します。
    /// </summary>
    /// <remarks>
    /// View側の処理なのでViewModelにはしません。
    /// </remarks>
    public async void Refresh()
    {
        if (this._vm == null)
        {
            return;
        }

        try
        {
            this.BeginInit();
            await this._vm.LoadAsync();

            var lastDate = new DateTime(this._vm.Year, this._vm.Month, 1).AddMonths(1).AddDays(-1);
            if (this._latestDate != lastDate)
            {
                // 年月が変わっていたら作り直す
                this.GenerateDataGridColumns(this.TabItemDataGrid, lastDate);
                this._latestDate = lastDate;
            }

            // Observable ではないため、設定し直します。
            this.TabItemDataGrid.ItemsSource = this._vm.Rows;
        }
        finally
        {

            this.EndInit();
        }
    }

    #region DataGrid

    /// <summary>
    /// スロットの数だけ DataGrid のヘッダーを作成します。
    /// 記号、氏名、備考がスロットの数だけ繰り返し作成します。
    /// </summary>
    private void GenerateDataGridColumns(DataGrid dataGrid, DateTime lastDate)
    {
        try
        {
            dataGrid.BeginInit();

            dataGrid.Columns.Clear();

            for (var i = 1; i <= lastDate.Day; i++)
            {
                dataGrid.Columns.Add(CreateDataGridColumn(lastDate.Year, lastDate.Month, i));
            }
        }
        finally
        {
            dataGrid.EndInit();
        }

        return;

        // local function
        static DataGridTemplateColumn CreateDataGridColumn(int year, int month, int day)
        {
            var date = new DateTime(year, month, day);
            //var col = new DataGridColumn
            //{
            //    Header = date.ToString("dd (ddd)"),
            //    Binding = new Binding($"DateCells[{date}]"),
            //};

            var bindingText = $"{nameof(SlotRowData.DateCells)}[{date}]";
            var xaml = $$"""
                <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                    <DockPanel>
                        <TextBlock DockPanel.Dock="Left" Text="{Binding {{bindingText}}.Symbol }" HorizontalAlignment="Center" />
                        <TextBlock DockPanel.Dock="Right" Text="{Binding {{bindingText}}.Remark}" HorizontalAlignment="Center" />
                        <Border DockPanel.Dock="Bottom" BorderThickness="1,0" BorderBrush="Gray">
                            <TextBlock Text="{Binding {{bindingText}}.Name}" HorizontalAlignment="Center" />
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
