using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AloeReservationGrid.App.ReservationApp.Views;

public class NamedDataGridTextColumn : DataGridTextColumn
{
    public string Name { get; set; }
}
