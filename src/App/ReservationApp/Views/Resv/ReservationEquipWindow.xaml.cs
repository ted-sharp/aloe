using AloeReservationGrid.App.ReservationApp.ViewModels;
using AloeReservationGrid.Lib.CoreLib.Util;
using System;
using System.Collections.Generic;
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

namespace AloeReservationGrid.App.ReservationApp.Views.Resv;

/// <summary>
/// ResvEquipWnd.xaml の相互作用ロジック
/// </summary>
public partial class ReservationEquipWindow : Window
{
    private readonly ReservationEquipViewModel _vm;

    public ReservationEquipWindow(ReservationEquipViewModel vm)
    {
        this.InitializeComponent();

        // なにか登録する
        //vm.RefreshAction = this.RefreshScheduleHeaders;

        this._vm = vm;
        this.DataContext = vm;

        this.InitializeSchedules();
    }


    private void InitializeSchedules()
    {
        // TODO: 有効な設備を取得する
        // 設備の数だけタブを作る

        // タブの数だけ以下の処理を行う
        // タブの中のヘッダーを生成する
        // データはItemSourceにバインドしておく

        // さくさく表示したいので、キャッシュする？
        // 数分以内なら前の状況のままでよさそう
        // F9/F10で次へ前へを連続しそうだし

        // DataGridのヘッダーは記号、氏名、備考
        // Gridのヘッダーは、時間が3colspan、その下に、記号、氏名、備考

        //this.GenerateBodyDataGrid("G0", this.BodyDataGrid0);
        //this.GenerateBodyDataGrid("G1", this.BodyDataGrid1);
        //this.GenerateBodyDataGrid("G2", this.BodyDataGrid2);

        //// StartMonth の変更がトリガなので、初回は明示的に呼び出す
        //var startDate = this._vm.StartMonth.Value.ToDateOrToday();
        //this.RefreshScheduleHeaders(startDate);
    }

    private void RefreshScheduleHeaders(DateTime startDate)
    {
        //Application.Current.Dispatcher.Invoke(() =>
        //{
        //    this.GenerateHeaderGrid("G0", this.HeaderGrid0, startDate);
        //    this.GenerateHeaderGrid("G1", this.HeaderGrid1, startDate);
        //    this.GenerateHeaderGrid("G2", this.HeaderGrid2, startDate);
        //});
    }

    #region DataGrid

    private void GenerateBodyDataGrid(string name, DataGrid dataGrid)
    {
        try
        {
            dataGrid.BeginInit();

            dataGrid.Columns.Clear();
            dataGrid.Columns.Add(this.CreateDataGridColumn($"{name}_Room", "Room"));

            // TODO: スロットの数分作る
            //var max = this._vm.OffsetDayCount.Value;
            //for (var i = 0; i < max; i++)
            //{
            //    dataGrid.Columns.Add(this.CreateDataGridColumn($"{name}_C{i}AM", "AM"));
            //    dataGrid.Columns.Add(this.CreateDataGridColumn($"{name}_C{i}PM", "PM"));
            //}
        }
        finally
        {
            dataGrid.EndInit();
        }
    }

    private DataGridTextColumn CreateDataGridColumn(string name, string header)
    {
        var col = new DataGridTextColumn { Header = header };
        this.RegisterName(name, col);
        return col;
    }

    #endregion DataGrid

    #region Grid

    private void GenerateHeaderGrid(string name, Grid grid, DateTime startDate)
    {
        try
        {
            grid.BeginInit();

            // 設備に登録されているSlotsの数
            var max = 10;

            //grid.Children.Clear();
            //grid.Children.Capacity = max * 2;

            //grid.RowDefinitions.Clear();
            //grid.ColumnDefinitions.Clear();

            //// 日付用
            //grid.RowDefinitions.Add(new RowDefinition());
            //// 曜日用
            //grid.RowDefinitions.Add(new RowDefinition());

            //grid.ColumnDefinitions.Add(ReservationMainWindow.CreateGridColumnDefinition($"{name}_Room"));

            //for (var i = 0; i < max; i++)
            //{
            //    grid.ColumnDefinitions.Add(ReservationMainWindow.CreateGridColumnDefinition($"{name}_C{i}AM"));
            //    grid.ColumnDefinitions.Add(ReservationMainWindow.CreateGridColumnDefinition($"{name}_C{i}PM"));

            //    var date = startDate.AddDays(i);
            //    grid.Children.Add(ReservationMainWindow.CreateGridDateLabel(date, i));
            //    grid.Children.Add(ReservationMainWindow.CreateGridDowLabel(date, i));
            //}

            //// 末尾の余白用
            //grid.ColumnDefinitions.Add(new ColumnDefinition());
        }
        finally
        {
            grid.EndInit();
        }
    }

    private static ColumnDefinition CreateGridColumnDefinition(string name)
    {
        //< ColumnDefinition Width = "{Binding ElementName=A1, Path=ActualWidth, Mode=OneWay}" />

        var col = new ColumnDefinition();
        var binding = new Binding
        {
            ElementName = name,
            Path = new PropertyPath("ActualWidth"),
            Mode = BindingMode.OneWay,
        };
        col.SetBinding(ColumnDefinition.WidthProperty, binding);
        return col;
    }

    private static Label CreateGridDateLabel(DateTime date, int offset)
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

    private static Label CreateGridDowLabel(DateTime date, int offset)
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

    #endregion Grid
}
