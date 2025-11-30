using NetTopologySuite.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Aloe.Medock.Reservation.AloeMedockResvApp.Utils;
using System.Reflection;
using System.Runtime.InteropServices;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Common.AloeCoreLib.Wpf.Extensions;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Views.Maint;
/// <summary>
/// ErrorWindow.xaml の相互作用ロジック
/// </summary>
public partial class ErrorWindow : Window
{
    public static void Show(Exception ex)
    {
        try
        {
            var errorWindow = new ErrorWindow();
            errorWindow.SetEnvironment();
            errorWindow.SetError(ex);
            errorWindow.ShowDialog();
        }
        catch
        {
            MessageBox.Show(
                "An unexpected error has occurred: " + ex.Message,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    public ErrorWindow()
    {
        this.InitializeComponent();
        this.Closed += App.Current.Window_OnClosed;
    }

    public void SetEnvironment()
    {
        this.UserEnvironmentTextBox.Text = Env.GetEnvironmentString();
    }

    public void SetError(Exception ex)
    {
        var msg = new StringBuilder();
        var details = new StringBuilder();
        if (ex is AggregateException aggEx)
        {
            foreach (var innerEx in aggEx.InnerExceptions)
            {
                msg.AppendLine($"{innerEx.GetType().Name}: {innerEx.Message}");
                details.AppendLine(innerEx.ToString()).AppendLine();
            }
        }
        else
        {
            msg.AppendLine($"{ex.GetType().Name}: {ex.Message}");
            details.AppendLine(ex.ToString()).AppendLine();
        }

        // エラーメッセージを表示
        this.ErrorMessageTextBlock.Text = msg.ToString();

        // TODO: Serilogのやつを組み立てて、カラー表示したいけど・・・？
        var paragraph = new Paragraph();

        var span = new Span();
        span.Inlines.Add(details.ToString());

        paragraph.Inlines.Add(span);

        // 詳細に追加
        this.DetailsRichTextBox.Document.Blocks.Clear();
        this.DetailsRichTextBox.Document.Blocks.Add(paragraph);
    }

    private void OpenDetailButton_OnClick(object sender, RoutedEventArgs e)
    {
        var current = this.DetailsRichTextBox.Visibility;
        // TODO: Visibility のトリガーでストーリーボードでアニメーションできたらよいかも？
        if (current == Visibility.Collapsed)
        {
            this.DetailsRichTextBox.Visibility = Visibility.Visible;
        }
        else
        {
            this.DetailsRichTextBox.Visibility = Visibility.Collapsed;
        }
    }

    private void OpenReportButton_OnClick(object sender, RoutedEventArgs e)
    {
        var current = this.ReportsGrid.Visibility;
        // TODO: Visibility のトリガーでストーリーボードでアニメーションできたらよいかも？
        if (current == Visibility.Collapsed)
        {
            this.ReportsGrid.Visibility = Visibility.Visible;
        }
        else
        {
            this.ReportsGrid.Visibility = Visibility.Collapsed;
        }
    }

    private void SendReportButton_OnClick(object sender, RoutedEventArgs e)
    {
        // TODO: メールサーバーの設定はコンフィグか、DBから取得してメールを送りたい。
        // メールが送れない状況なら、サーバーへ記録しておきたい
        // MailKit を使う
        MessageBox.Show(this.DetailsRichTextBox.GetText());
    }
}
