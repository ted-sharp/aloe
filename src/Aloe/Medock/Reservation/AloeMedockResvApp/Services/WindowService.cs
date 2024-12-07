using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Services;
public class WindowService
{
    private readonly IServiceProvider _services;

    public WindowService(IServiceProvider services)
    {
        this._services = services;
    }

    private T? Resolve<T>()
    {
        return this._services.GetService<T>();
    }

    public T CreateWindow<T>()
        where T : Window
    {
        var window = this.Resolve<T>();

        var type = typeof(T);
        return window ?? throw new Exception($"Can Not Create Window. (Type: {type})");
    }

    public T? GetWindow<T>()
        where T : Window
    {
        var window = Application.Current.Windows
            .OfType<T>()
            .FirstOrDefault();

        return window;
    }

    public T GetOrCreateWindow<T>()
        where T : Window
    {
        var window = this.GetWindow<T>()
                     ?? this.CreateWindow<T>();

        return window;
    }
}
