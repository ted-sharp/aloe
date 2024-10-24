using System.Diagnostics;
using System.Numerics;
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
using AloeReservationGrid.Lib.ReservationLib.Configuation;
using AloeReservationGrid.Lib.ReservationLib.Rpc;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion;
using MagicOnion.Client;
using Microsoft.Extensions.Options;

namespace AloeReservationGrid.App.ReservationApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly GrpcConfig _grpcConfig;

    public MainWindow(IOptions<GrpcConfig> grpcConfig)
    {
        this.InitializeComponent();

        this._grpcConfig = grpcConfig.Value;
    }

    private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        // Connect to the server using gRPC channel.
        var channel = GrpcChannel.ForAddress($"http://{this._grpcConfig.IPAddress}:{this._grpcConfig.Port}");

        // NOTE: If your project targets non-.NET Standard 2.1, use `Grpc.Core.Channel` class instead.
        // var channel = new Channel("localhost", 5001, new SslCredentials());

        // Create a proxy to call the server transparently.
        var client = MagicOnionClient.Create<IMyFirstService>(channel);

        // Call the server-side method using the proxy.
        var result = await client.SumAsync(123, 456);
        Console.WriteLine($"Result: {result}");
    }

    private async void ButtonBase2_OnClick(object sender, RoutedEventArgs e)
    {
        var channel = GrpcChannel.ForAddress($"http://{this._grpcConfig.IPAddress}:{this._grpcConfig.Port}");
        var client = await StreamingHubClient.ConnectAsync<IGamingHub, IGamingHubReceiver>(channel, this._receiver);

        // 通常のメソッド呼び出しのように AddAsync を使用
        var result = await client.JoinAsync("room 1", "user 1");
        Console.WriteLine($"Result of AddAsync: {result[0].Name}"); // 出力: Result of AddAsync: 30

        // 必要に応じてStreamingHubの接続を切断
        await client.LeaveAsync();
        await client.DisposeAsync();
    }

    private readonly GamingHubReceiver _receiver = new();

    // 双方向なので、クライアント側へも実装が必要
    public class GamingHubReceiver : IGamingHubReceiver
    {
        // サーバーからプレイヤーが参加したことを通知された際の処理
        public void OnJoin(Player player)
        {
            if (player != null)
            {
                Console.WriteLine($"{player.Name} has joined the game.");
            }
            else
            {
                Console.WriteLine("Received a null player on join.");
            }
        }

        // サーバーからプレイヤーが退出したことを通知された際の処理
        public void OnLeave(Player player)
        {
            if (player != null)
            {
                Console.WriteLine($"{player.Name} has left the game.");
            }
            else
            {
                Console.WriteLine("Received a null player on leave.");
            }
        }
    }
}
