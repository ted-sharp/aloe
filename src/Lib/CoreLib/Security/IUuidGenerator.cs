using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.CoreLib.Security;

public interface IUuidGenerator
{
    Guid NewGuid();
}
