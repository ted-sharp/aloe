using Aloe.Utils.Drawing.Wpf.Samples.Assets;
using System.Windows;

namespace Aloe.Utils.Drawing.Wpf.Samples;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
    }

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        this.Icon = Icons.Aloe.Value.ToImageSource();
    }
}
