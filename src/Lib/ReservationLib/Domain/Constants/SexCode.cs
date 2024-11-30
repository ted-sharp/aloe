using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Domain.Constants;

/// <summary>
/// ISO 5218
/// </summary>
public enum SexCode : int
{
    NotKnown = 0,
    Male = 1,
    Female = 2,
    NotApplicable = 9,
}
