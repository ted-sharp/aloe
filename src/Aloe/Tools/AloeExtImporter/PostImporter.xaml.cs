using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Windows.Controls;

namespace AloeExtImporter;

/// <summary>
/// PostImporter.xaml の相互作用ロジック
/// </summary>
public partial class PostImporter : UserControl
{
    public PostImporter()
    {
        this.InitializeComponent();
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        FileHelper.OpenLink(e.Uri.AbsoluteUri);
        e.Handled = true;
    }

    private void OpenWorkButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var folderPath = FileHelper.PickFolder(this.WorkTextBox.Text);
            if (!String.IsNullOrWhiteSpace(folderPath)
                && Directory.Exists(folderPath))
            {
                this.WorkTextBox.Text = folderPath;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
        }
    }

    private async void DownloadPostButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var url = this.DownloadUrlTextBox.Text ?? "";
            var localFileDir = this.WorkTextBox.Text ?? "";
            var downloadFilePath = FileHelper.GetDownloadFilePath(url, localFileDir);

            if (!Directory.Exists(localFileDir))
            {
                Directory.CreateDirectory(localFileDir);
            }

            var progress = new Progress<int>(percent =>
            {
                this.ProgressBar.Value = percent;
                this.ProgressBar.IsEnabled = true;
            });

            var factory = App.Host.Services.GetRequiredService<IHttpClientFactory>();
            var downloader = new FileDownloader(factory.CreateClient());
            await downloader.DownloadFileAsync(url, downloadFilePath, progress);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
        }
        finally
        {
            this.ProgressBar.IsEnabled = false;
        }
    }

    private async void ImportPostButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            // 進捗バーをインジケーター表示に設定
            this.ProgressBar.IsIndeterminate = true;

            var url = this.DownloadUrlTextBox.Text ?? "";
            var localFileDir = this.WorkTextBox.Text ?? "";
            var downloadFilePath = FileHelper.GetDownloadFilePath(url, localFileDir);

            // ZIPファイルからCSVファイルパスを取得
            var extractedCsvPath = ZipExtractor.ExtractFirstCsv(downloadFilePath, localFileDir);

            // CSVインポート処理
            var importer = new CsvImporter();
            await Task.Run(() => importer.ImportPostCsvToDatabase(extractedCsvPath));

            // ファイル削除
            FileHelper.DeleteFileIfExists(extractedCsvPath);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
        }
        finally
        {
            this.ProgressBar.Value = this.ProgressBar.Maximum;
            this.ProgressBar.IsIndeterminate = false;
        }
    }
}
