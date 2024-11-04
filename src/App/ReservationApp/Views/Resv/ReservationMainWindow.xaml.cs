using System.Diagnostics;
using System.Windows;
using AloeReservationGrid.Lib.ReservationLib.Configuation;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Services;
using Grpc.Net.Client;
using MagicOnion.Client;
using Microsoft.Extensions.Options;

namespace AloeReservationGrid.App.ReservationApp.Views.Resv;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class ReservationMainWindow : Window
{
    private readonly GrpcConfig _grpcConfig;

    public ReservationMainWindow(IOptions<GrpcConfig> grpcConfig)
    {
        this.InitializeComponent();

        this._grpcConfig = grpcConfig.Value;
    }

    private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        // Connect to the server using gRPC channel.
        var channel = GrpcChannel.ForAddress(this._grpcConfig.Url);

        // Create a proxy to call the server transparently.
        var client = MagicOnionClient.Create<IMyFirstService>(channel);

        // Call the server-side method using the proxy.
        var result = await client.SumAsync(123, 456);
        Debug.WriteLine($"Result: {result}");
    }

    private async void ButtonBase2_OnClick(object sender, RoutedEventArgs e)
    {
        //var channel = GrpcChannel.ForAddress(this._grpcConfig.Url);
        //var client = await StreamingHubClient.ConnectAsync<IGamingHub, IGamingHubReceiver>(channel, this._receiver);

        //// 通常のメソッド呼び出しのように AddAsync を使用
        //var result = await client.JoinAsync("room 1", "user 1");
        //Debug.WriteLine($"Result of AddAsync: {result[0].Name}"); // 出力: Result of AddAsync: 30

        //// 必要に応じてStreamingHubの接続を切断
        //await client.LeaveAsync();
        //await client.DisposeAsync();
    }

    //private readonly GamingHubReceiver _receiver = new();

    //// 双方向なので、クライアント側へも実装が必要
    //public class GamingHubReceiver : IGamingHubReceiver
    //{
    //    // サーバーからプレイヤーが参加したことを通知された際の処理
    //    public void OnJoin(Player player)
    //    {
    //        if (player != null)
    //        {
    //            Debug.WriteLine($"{player.Name} has joined the game.");
    //        }
    //        else
    //        {
    //            Debug.WriteLine("Received a null player on join.");
    //        }
    //    }

    //    // サーバーからプレイヤーが退出したことを通知された際の処理
    //    public void OnLeave(Player player)
    //    {
    //        if (player != null)
    //        {
    //            Debug.WriteLine($"{player.Name} has left the game.");
    //        }
    //        else
    //        {
    //            Debug.WriteLine("Received a null player on leave.");
    //        }
    //    }
    //}
}
