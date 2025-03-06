using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aloe.Common.AloeCoreLib.Client.Mvvm;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;
using Microsoft.Extensions.Logging;
using Reactive.Bindings.Extensions;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;

// 基本的に View 側への参照は持たない。
// 他のWindowを使いたいときはDIしたサービス経由で使うことになる。
// 慣例として ViewModelBase が継承しているインターフェースも列挙します。
public class SampleViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
{
    /// <summary>
    /// バインド用のプロパティです。
    /// </summary>
    /// <remarks>
    /// <example>
    /// How to use:
    /// <code>
    /// <TextBox Text="{Binding SampleProperty.Value, UpdateSourceTrigger=PropertyChanged, Mode=OneWayToSource}" />
    /// </code>
    /// </example>
    /// </remarks>
    public ReactivePropertySlim<string> SampleProperty { get; } = new("any");

    /// <summary>
    /// バインド用のコマンドです。
    /// </summary>
    /// <remarks>
    /// <example>
    /// How to use:
    /// <code>
    /// <Button Content= "Execute Sample" Command= "{Binding SampleCommand}" />
    /// </code>
    /// </example>
    /// </remarks>
    public ReactiveCommandSlim SampleCommand { get; } = new();

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

        this.SampleProperty
            .Subscribe(this.Method)
            .AddTo(this.Disposables);

        this.SampleCommand
            .Subscribe(this.CommandMethod)
            .AddTo(this.Disposables);
    }

    // async void の場合は、ラムダ式ではなくメソッド化しておくとシンプルになる
    private async void Method(string propValue)
    {
        try
        {
            await Task.Delay(1000);

            this._logger.LogInformation(propValue);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, ex.Message);
        }
    }

    private async void CommandMethod()
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
