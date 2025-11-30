using R3;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;
using ObservableCollections;
using Aloe.Common.AloeCoreLib.Mvvm;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;

// 基本的に View 側への参照は持たない。
// 他のWindowを使いたいときはDIしたサービス経由で使うことになる。
// 慣例として ViewModelBase が継承しているインターフェースも列挙します。
public class SampleViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
{
    /// <summary>
    /// 選択中の項目インデックスです。
    /// </summary>
    /// <remarks>
    /// 検索のときに参照します。
    /// 検索のときに使うだけなので OneWayToSource とします。
    /// イベントを発火させないので ReactiveProperty にはしません。
    /// </remarks>
    /// <example>
    /// How to use:
    /// <code>
    /// <TextBox Text="{Binding SelectedIndexInput, Mode=OneWayToSource}" />
    /// </code>
    /// </example>
    public int SelectedIndexInput { get; set; } = -1;

    /// <summary>
    /// 入力されたコードです。
    /// </summary>
    /// <remarks>
    /// 入力をうけてイベントを発火します。
    /// イベント発火に使うだけなので OneWayToSource とします。
    /// イベントを発火させるので ReactiveProperty にします。
    /// 変更されたとき、フロア名、を更新します。
    /// </remarks>
    /// <example>
    /// How to use:
    /// <code>
    /// <TextBox Text="{Binding XxxCode, Mode=OneWayToSource}" />
    /// TextBox.Text はデフォルトで Mode=TwoWay となります。
    /// </code>
    /// </example>
    public ReactiveProperty<string> XxxCode { get; set; } = new();

    /// <summary>
    /// 入力されたコードの名前です。
    /// </summary>
    /// <remarks>
    /// 更新した値を表示するだけなので OneWay とします。
    /// INotifyPropertyChanged が必要なので BindableReactiveProperty にします。
    /// </remarks>
    /// <example>
    /// How to use:
    /// <code>
    /// <TextBlock Text="{Binding XxxName}" />
    /// TextBlock.Text はデフォルトで Mode=OneWay となります。
    /// </code>
    /// </example>
    public BindableReactiveProperty<string> XxxName { get; set; } = new();

    /// <summary>
    /// バインド用のコマンドです。
    /// </summary>
    /// <example>
    /// How to use:
    /// <code>
    /// <Button Content= "Execute Sample" Command= "{Binding SampleCommand}" />
    /// </code>
    /// </example>
    public ReactiveCommand SampleCommand { get; } = new();

    //public ObservableList<ReservationDailyNoteDto> ReservationDailyNotes { get; set; } = new();

    // View 側で設定する Close アクション
    public Action? CloseAction { get; set; }

    private readonly ILogger _logger;
    private readonly ISampleGrpcService _sampleGrpcService;

    public SampleViewModel(
        ILogger<SampleViewModel> logger,
        ISampleGrpcService sampleGrpcService)
    {
        this._logger = logger;
        this._sampleGrpcService = sampleGrpcService;

        // Dispose の方法はいくつかある
        // https://github.com/Cysharp/R3?tab=readme-ov-file#disposable
        var d = R3.Disposable.CreateBuilder();

        this.XxxCode
            .Subscribe(this.Method)
            .AddTo(ref d);

        this.SampleCommand
            .Subscribe(this.CommandMethod)
            .AddTo(ref d);

        this.Disposable = d.Build();
    }

    // async void の場合は、ラムダ式ではなくメソッド化しておくとシンプルになる
    private async void Method(string propValue)
    {
        try
        {
            await Task.Delay(1000);

            this.XxxName.Value = propValue;

            this._logger.LogInformation(propValue);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, ex.Message);
        }
    }

    private async void CommandMethod(Unit _)
    {
        try
        {
            var sampleDto = await this._sampleGrpcService.FetchSampleAsync();
            // なにかする

            // 最後にウインドウを閉じる
            this.CloseAction?.Invoke();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, ex.Message);
        }
    }
}
