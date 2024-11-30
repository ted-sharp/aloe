using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;
using Microsoft.Extensions.DependencyInjection;

namespace AloeReservationGrid.App.ReservationApp;

public partial class App
{
    #region Global

    #region Global / Resolve

    private static IServiceProvider? s_services;

    public static IServiceProvider Services
    {
        get => App.s_services ?? throw new InvalidOperationException("Service provider is not initialized.");
        private set => App.s_services = value ?? throw new ArgumentNullException(nameof(value));
    }

    public static T? Resolve<T>()
    {
        return App.Services.GetService<T>();
    }

    #endregion  Global / Resolve

    public static SessionDto? Session { get; set; }

    public static bool HasSession => App.Session != null;


    #endregion Global

}
