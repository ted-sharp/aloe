using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.ReservationLib.Configuation;

public class GrpcConfig
{
    public string IPAddress { get; set; }
    public int Port { get; set; }
    public bool UseSsl { get; set; }

    public IPAddress ParseIPAddress()
    {
        try
        {
            if (System.Net.IPAddress.TryParse(this.IPAddress, out var addr))
            {
                return addr;
            }
        }
        catch
        {
            // 何もしない
        }
        return System.Net.IPAddress.None;
    }
}


