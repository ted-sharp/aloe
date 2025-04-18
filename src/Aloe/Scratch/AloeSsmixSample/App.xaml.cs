using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.Hosting;

namespace AloeSsmixSample;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IHost Host { get; private set; } = null!;

    private void App_OnStartup(object sender, StartupEventArgs e)
    {
        var args = Environment.GetCommandLineArgs();

        App.Host = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args)
            .ConfigureAloeSsmixSample()
            .Build();
    }
}

