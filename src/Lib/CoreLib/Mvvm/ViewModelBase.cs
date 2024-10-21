using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeReservationGrid.Lib.CoreLib.Mvvm;

// フィールド 'PropertyChanged' が割り当てられていますが、値は使用されていません
#pragma warning disable CS0414

// ViewModel では ReactiveProperty を使うので INotifyPropertyChanged は不要ですが、
// DataContext にいれるものは INotifyPropertyChanged を実装しておかないとメモリリークします。
public class ViewModelBase : INotifyPropertyChanged, IDisposable
{
    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged = null;

    #endregion INotifyPropertyChanged

    #region IDisposable

    private bool _disposed = false;

    public void Dispose()
    {
        if (!this._disposed)
        {
            this.PropertyChanged = null;
            this.Disposables.Dispose();
            this._disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    // Compositeパターンの変数には複数形を用います。
    protected System.Reactive.Disposables.CompositeDisposable Disposables { get; } = [];
}
