using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Views;

public class NamedDataGridTextColumn : DataGridTextColumn
{
    public required string Name { get; set; }
}
