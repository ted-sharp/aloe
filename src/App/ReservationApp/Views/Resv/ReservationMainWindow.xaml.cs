using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using AloeReservationGrid.App.ReservationApp.ViewModels;
using AloeReservationGrid.App.ReservationApp.Views.Login;
using AloeReservationGrid.Lib.ReservationLib.Configuation;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Services;
using Grpc.Net.Client;
using MagicOnion.Client;
using Microsoft.Extensions.Options;

namespace AloeReservationGrid.App.ReservationApp.Views.Resv;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class ReservationMainWindow : Window
{
    private ReservationMainViewModel _vm;

    public ReservationMainWindow(ReservationMainViewModel vm)
    {
        this.InitializeComponent();

        this._vm = vm;
        this.DataContext = vm;

        this.GenerateBodyDataGrid();
        this.GenerateHeaderGrid();
    }

    private void GenerateBodyDataGrid()
    {
        this.BodyDataGrid.Columns.Clear();
        this.BodyDataGrid.Columns.Add(this.CreateDataGridColumn("Room", "Room"));

        var max = this._vm.OffsetDayCount.Value;
        for (var i = 0; i < max; i++)
        {
            this.BodyDataGrid.Columns.Add(this.CreateDataGridColumn($"C{i}AM", "AM"));
            this.BodyDataGrid.Columns.Add(this.CreateDataGridColumn($"C{i}PM", "PM"));
        }
    }

    private DataGridTextColumn CreateDataGridColumn(string name, string header)
    {
        var col = new DataGridTextColumn { Header = header };
        this.RegisterName(name, col);
        return col;
    }

    private void GenerateHeaderGrid()
    {
        this.HeaderGrid.ColumnDefinitions.Clear();

        var currentDate = this._vm.StartDate.Value;
        var max = this._vm.OffsetDayCount.Value;
        for (var i = 0; i < max; i++)
        {
            this.HeaderGrid.ColumnDefinitions.Add(this.CreateGridColumnDefinition($"C{i}AM"));
            this.HeaderGrid.ColumnDefinitions.Add(this.CreateGridColumnDefinition($"C{i}PM"));
            this.HeaderGrid.Children.Add(this.CreateGridLabel(i, currentDate));
        }
    }

    private ColumnDefinition CreateGridColumnDefinition(string name)
    {
        //< ColumnDefinition Width = "{Binding ElementName=A1, Path=ActualWidth, Mode=OneWay}" />

        var col = new ColumnDefinition();
        var binding = new Binding
        {
            ElementName = name,
            Path = new PropertyPath("ActualWidth"),
            Mode = BindingMode.OneWay,
        };
        //col.SetBinding(col.Width, binding);
        return col;
    }

    private Label CreateGridLabel(int i, DateTime currentDate)
    {
        //<Label Grid.ColumnSpan="3" Grid.Column= "0" Content= "First dude" />

        var lbl = new Label
        {
            Content = currentDate.AddDays(i).ToString("MM/dd"),
        };
        Grid.SetColumn(lbl, i * 2);
        Grid.SetColumnSpan(lbl, 2);
        return lbl;
    }




}
