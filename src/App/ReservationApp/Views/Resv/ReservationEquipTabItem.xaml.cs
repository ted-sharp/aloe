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
using System.Windows.Navigation;
using System.Windows.Shapes;
using AloeReservationGrid.App.ReservationApp.Utils;
using AloeReservationGrid.App.ReservationApp.ViewModels;

namespace AloeReservationGrid.App.ReservationApp.Views.Resv;

public partial class ReservationEquipTabItem : UserControl
{
    private readonly ScrollViewerSynchronizer _synchronizer = new();

    private ReservationEquipTabItemViewModel? Vm => this.DataContext as ReservationEquipTabItemViewModel;

    private List<string> _latestSlots = null!;

    public ReservationEquipTabItem()
    {
        this.InitializeComponent();
    }

    private void ReservationEquipTabItem_OnLoaded(object sender, RoutedEventArgs e)
    {
        // ScrollViewer を同期対象に追加
        this._synchronizer.AddScrollViewer(this.ScrollViewer);

        var view = ScrollViewerSynchronizer.FindChildScrollViewer(this.TabItemDataGrid);
        if (view != null)
        {
            this._synchronizer.AddScrollViewer(view);
        }
    }

    private void ReservationEquipTabItem_OnUnloaded(object sender, RoutedEventArgs e)
    {
        this._synchronizer.Clear();
    }

    private void ReservationEquipTabItem_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (this.Vm != null)
        {
            this.Vm.RefreshAction = this.Refresh;
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
        if (this.Vm == null)
        {
            return;
        }

        try
        {
            this.BeginInit();

            await this.Vm.LoadAsync();

            var equipName = this.Vm.EquipName.ReplaceInvalidNameChars();
            var slots = this.Vm.Slots;

            if (this._latestSlots == null! ||
                !this._latestSlots.SequenceEqual(slots))
            {
                // スロットの数だけ DataGrid カラム(通常のDataGridのヘッダー)を作成します。
                this.GenerateDataGridColumns(this.TabItemDataGrid, equipName, slots.Count);
                this._latestSlots = slots;

                // スロットの数だけ Grid カラム(カスタムヘッダー)を作成します。
                this.GenerateGridColumns(this.ColumnGrid, equipName, slots);
            }

            // Observable ではないため、設定し直します。
            this.TabItemDataGrid.ItemsSource = this.Vm.DataTable.DefaultView;
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
    private void GenerateDataGridColumns(DataGrid dataGrid, string equipName, int slotCount)
    {
        try
        {
            dataGrid.BeginInit();

            ClearDataGridColumn(this, dataGrid);

            for (var i = 0; i < slotCount; i++)
            {
                dataGrid.Columns.Add(CreateDataGridColumn($"{equipName}_C{i}_symbol", "記号"));
                dataGrid.Columns.Add(CreateDataGridColumn($"{equipName}_C{i}_fullname", "氏名"));
                dataGrid.Columns.Add(CreateDataGridColumn($"{equipName}_C{i}_remark", "備考"));
            }
        }
        finally
        {
            dataGrid.EndInit();
        }

        return;

        // local function
        static void ClearDataGridColumn(FrameworkElement self, DataGrid target)
        {
            var cols = target.Columns.OfType<NamedDataGridTextColumn>();
            foreach (var col in cols)
            {
                // 解除しないと重複登録で例外が発生します。
                self.UnregisterName(col.Name);
            }
            target.Columns.Clear();
        }

        // local function
        DataGridTextColumn CreateDataGridColumn(string columnName, string header)
        {
            var col = new NamedDataGridTextColumn { Name = columnName, Header = header };
            // バインド用に名前を登録します。
            this.RegisterName(columnName, col);
            return col;
        }
    }

    #endregion DataGrid

    #region Grid

    /// <summary>
    /// スロットの数だけ Grid のヘッダーを作成します。
    /// 記号、氏名、備考にまたがるスロット名のラベルを作成します。
    /// </summary>
    private void GenerateGridColumns(Grid grid, string equipName, List<string> slots)
    {
        try
        {
            grid.BeginInit();

            grid.Children.Clear();
            grid.Children.Capacity = slots.Count;

            grid.ColumnDefinitions.Clear();

            // 日付、曜日、合計を表示するRowHeader分のスペース
            //grid.ColumnDefinitions.Add(CreateGridColumnDefinition($"{equipName}_CornerHeader"));

            for (var i = 0; i < slots.Count; i++)
            {
                grid.ColumnDefinitions.Add(CreateGridColumnDefinition($"{equipName}_C{i}_symbol"));
                grid.ColumnDefinitions.Add(CreateGridColumnDefinition($"{equipName}_C{i}_fullname"));
                grid.ColumnDefinitions.Add(CreateGridColumnDefinition($"{equipName}_C{i}_remark"));

                var slot = slots[i];
                grid.Children.Add(CreateGridLabel(slot, i));
            }

            // 末尾の余白用
            grid.ColumnDefinitions.Add(CreateGridColumnDefinitionStar());
        }
        finally
        {
            grid.EndInit();
        }

        return;

        // local function
        static ColumnDefinition CreateGridColumnDefinitionStar()
        {
            return new ColumnDefinition()
            {
                // 1* を設定
                Width = new GridLength(1, GridUnitType.Star),
            };
        }

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
        static Label CreateGridLabel(string slot, int index)
        {
            var lbl = new Label
            {
                Content = slot.TrimAfterBrackets(),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };

            Grid.SetColumn(lbl, index * 3);
            Grid.SetColumnSpan(lbl, 3);

            return lbl;
        }
    }

    #endregion Grid
}
