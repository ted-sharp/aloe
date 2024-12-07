
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Net;
using System.Reflection.PortableExecutable;
using Microsoft.EntityFrameworkCore;

using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;
using MagicOnion.Server;
using MagicOnion;
using Aloe.Common.AloeCoreLib.Security;
using MagicOnion.Serialization.MessagePack;
using MagicOnion.Serialization;

namespace Aloe.Medock.Reservation.AloeMedockResvServer;

internal static class Program
{
    internal static void Main(string[] args)
    {
        var host = WebApplication.CreateSlimBuilder(args)
            .ConfigureBuilder()
            .ConfigureKestrel()
            .Build();

        host.ConfigureApp()
            .Run();
    }
}
