using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Aloe.Apps.MedockClient;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await this.InitializeWebViewAsync();
    }

    private async Task InitializeWebViewAsync()
    {
        var env = await CoreWebView2Environment.CreateAsync();
        await this.WebView.EnsureCoreWebView2Async(env);
        this.WebView.Source = new Uri("http://localhost:5084");
    }
}
