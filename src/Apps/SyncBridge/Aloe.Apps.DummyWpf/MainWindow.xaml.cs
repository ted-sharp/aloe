using System.Reflection;
using System.Text;
using System.Windows;

namespace Aloe.Apps.DummyWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.LoadApplicationInfo();
        }

        private void LoadApplicationInfo()
        {
            var assembly = Assembly.GetExecutingAssembly();
            this.AssemblyNameText.Text = assembly.GetName().Name ?? "Unknown";
            this.VersionText.Text = assembly.GetName().Version?.ToString() ?? "Unknown";
            this.WorkingDirectoryText.Text = Environment.CurrentDirectory;

            var args = Environment.GetCommandLineArgs();
            this.ArgsCountText.Text = $"Arguments Count: {args.Length}";

            var argsBuilder = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                argsBuilder.AppendLine($"[{i}] {args[i]}");
            }
            this.ArgsTextBox.Text = argsBuilder.ToString();

            this.LoadEnvironmentVariables();
        }

        private void LoadEnvironmentVariables()
        {
            var envBuilder = new StringBuilder();
            var envVars = Environment.GetEnvironmentVariables();

            var sortedKeys = new List<string>();
            foreach (var key in envVars.Keys)
            {
                sortedKeys.Add(key.ToString() ?? String.Empty);
            }
            sortedKeys.Sort();

            foreach (var key in sortedKeys)
            {
                var value = envVars[key];
                envBuilder.AppendLine($"{key} = {value}");
            }

            this.EnvironmentTextBox.Text = envBuilder.ToString();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            this.LoadApplicationInfo();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
