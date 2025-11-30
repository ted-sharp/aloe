using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Utils;
public class ScrollViewerSynchronizer : IDisposable
{
    private readonly List<ScrollViewer> _scrollViewers = new();
    private bool _isScrollSyncing;

    /// <summary>
    /// ScrollViewer を追加します。
    /// </summary>
    /// <param name="scrollViewer">同期対象の ScrollViewer。</param>
    public void AddScrollViewer(ScrollViewer scrollViewer)
    {
        if (!this._scrollViewers.Contains(scrollViewer))
        {
            this._scrollViewers.Add(scrollViewer);
            scrollViewer.ScrollChanged += this.ScrollViewer_OnScrollChanged;
        }
    }

    /// <summary>
    /// ScrollViewer を削除します。
    /// </summary>
    /// <param name="scrollViewer">同期対象から除外する ScrollViewer。</param>
    public void RemoveScrollViewer(ScrollViewer scrollViewer)
    {
        if (this._scrollViewers.Contains(scrollViewer))
        {
            scrollViewer.ScrollChanged -= this.ScrollViewer_OnScrollChanged;
            this._scrollViewers.Remove(scrollViewer);
        }
    }

    /// <summary>
    /// すべての ScrollViewer を同期対象から削除します。
    /// </summary>
    public void Clear()
    {
        foreach (var scrollViewer in this._scrollViewers)
        {
            scrollViewer.ScrollChanged -= this.ScrollViewer_OnScrollChanged;
        }

        this._scrollViewers.Clear();
    }

    private void ScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (this._isScrollSyncing)
        {
            return;
        }

        try
        {
            this._isScrollSyncing = true;

            if (sender is not ScrollViewer source)
            {
                return;
            }

            var horizontalOffset = source.HorizontalOffset;

            foreach (var scrollViewer in this._scrollViewers)
            {
                if (scrollViewer != source)
                {
                    scrollViewer.ScrollToHorizontalOffset(horizontalOffset);
                }
            }
        }
        finally
        {
            this._isScrollSyncing = false;
        }
    }

    public void Dispose()
    {
        this.Clear();
    }

    /// <summary>
    /// ScrollViewer を探します。
    /// </summary>
    public static ScrollViewer? FindChildScrollViewer(DependencyObject parent)
    {
        // VisualTreeHelper を使って DataGrid 内部の ScrollViewer を検索
        if (parent is ScrollViewer scrollViewer)
        {
            return scrollViewer;
        }

        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            var result = FindChildScrollViewer(child);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}
