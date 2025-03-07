using System.ComponentModel;

namespace Aloe.Common.AloeCoreLib.Client.Mvvm;

// フィールド 'PropertyChanged' が割り当てられていますが、値は使用されていません
#pragma warning disable CS0414

// ViewModel では ReactiveProperty を使うので INotifyPropertyChanged は不要ですが、
// DataContext にいれるものは INotifyPropertyChanged を実装しておかないとメモリリークします。
public class ViewModelBase : INotifyPropertyChanged, IDisposable
{
    #region INotifyPropertyChanged

    // 未使用ですが、INotifyPropertyChanged の実装に必要です。
    public event PropertyChangedEventHandler? PropertyChanged = null;

    #endregion INotifyPropertyChanged

    #region IDisposable

    protected IDisposable? Disposable { get; set; }

    private bool _disposed = false;

    public void Dispose()
    {
        if (!this._disposed)
        {
            this.PropertyChanged = null;
            this.Disposable?.Dispose();
            this._disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable
}
