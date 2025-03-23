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
using System.Security.Policy;

namespace AloeExtImporter;

/// <summary>
/// HoujinImporter.xaml の相互作用ロジック
/// </summary>
public partial class HoujinImporter : UserControl
{
    public HoujinImporter()
    {
        this.InitializeComponent();
    }

    private void HoujinImporter_OnLoaded(object sender, RoutedEventArgs e)
    {

        // TODO: 特定のファイル名を探してTextBoxに設定しておく
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

    private void OpenHoujinButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var filePath = FileHelper.PickFile(this.WorkTextBox.Text);
            if (!String.IsNullOrWhiteSpace(filePath)
                && File.Exists(filePath))
            {
                this.HoujinTextBox.Text = filePath;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
        }
    }

    private async void DownloadJisButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var url = this.JisDownloadUrlTextBox.Text ?? "";
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

    private async void ImportHoujinButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            // 進捗バーをインジケーター表示に設定
            this.ProgressBar.IsIndeterminate = true;

            var localFileDir = this.WorkTextBox.Text ?? "";

            #region Houjin

            var houjinFilePath = this.HoujinTextBox.Text ?? "";

            if (File.Exists(houjinFilePath))
            {
                // ZIPファイルからCSVファイルパスを取得
                var extractedCsvPath = ZipExtractor.ExtractFirstCsv(houjinFilePath, localFileDir);

                // CSVインポート処理
                var importer = new CsvImporter();
                await Task.Run(() => importer.ImportHoujinCsvToDatabase(extractedCsvPath));

                // ファイル削除
                FileHelper.DeleteFileIfExists(extractedCsvPath);
            }

            #endregion Houjin

            #region Jis

            var url = this.JisDownloadUrlTextBox.Text ?? "";
            var jisFilePath = FileHelper.GetDownloadFilePath(url, localFileDir);

            if (File.Exists(jisFilePath))
            {
                var importer2 = new ExcelImporter();
                await Task.Run(() => importer2.ImportJisExelToDatabase(jisFilePath));
            }

            #endregion Jis
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
