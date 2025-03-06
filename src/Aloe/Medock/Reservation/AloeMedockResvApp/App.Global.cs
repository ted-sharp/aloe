using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Aloe.Medock.Reservation.AloeMedockResvApp;

public partial class App
{
    #region Global

    public static readonly AssemblyName AsmName = Assembly.GetExecutingAssembly().GetName();

    public static readonly string AppVersion = $"v{App.AsmName.Version?.Major ?? 0}.{App.AsmName.Version?.Minor ?? 0}";

    public static readonly string AppName = $"{App.AsmName.Name} {App.AppVersion}";

    public static readonly string IniFilePath =
        System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            App.AsmName.Name ?? "AloeMedockResvApp",
            "app.ini");

    #region Global / Resolve

    public IHost Host => this._host
        ?? throw new InvalidOperationException("IHost is not initialized.");

    private static IServiceProvider? s_services;

    public static IServiceProvider Services => App.s_services
        ?? throw new InvalidOperationException("IServiceProvider is not initialized.");

    public static T Resolve<T>()
        where T : notnull
    {
        return App.Services.GetRequiredService<T>()
            ?? throw new InvalidOperationException($"{typeof(T).Name} can not resolve.");
    }

    #endregion  Global / Resolve

    public static string HostName { get; set; } = "";

    public static string DatabaseName { get; set; } = "";

    public static string HostUrl { get; set; } = "";

    #region Global / Session

    public static SessionDto? Session { get; set; }

    public static bool HasSession => App.Session != null;

    public static async Task<bool> TryLogoutAsync()
    {
        var session = App.Session;
        if (session is null)
        {
            return false;
        }

        var auth = App.Resolve<IAuthGrpcService>();
        await auth.LogoutAsync(session);
        App.Session = null;
        return true;
    }

    #endregion Global / Session

    #endregion Global
}
