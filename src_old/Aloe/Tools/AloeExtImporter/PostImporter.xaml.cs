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

    private void PostImporter_OnLoaded(object sender, RoutedEventArgs e)
    {
        this.RefreshDownloadStatus();
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            FileHelper.OpenLink(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
        }
    }

    private void BrowseWorkButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var work = this.GetWorkDirectory();
            var folderPath = FileHelper.PickFolder(work);
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

    private void OpenWorkButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var work = this.GetWorkDirectory();
            FileHelper.OpenExplorer(work);
            e.Handled = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
        }
    }

    private async void ImportPostButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            this.SetDownloadStatus(State.None);
            this.SetImportStatus(State.None);

            await this.DownloadFile();
            await this.ImportFile();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
        }
    }

    private string GetWorkDirectory()
    {
        var work = this.WorkTextBox.Text;
        return String.IsNullOrWhiteSpace(work) ? "./tmp/" : work;
    }

    private void RefreshDownloadStatus()
    {
        var url = this.DownloadUrlTextBox.Text ?? "";
        var work = this.GetWorkDirectory();
        var downloadFilePath = FileHelper.GetDownloadFilePath(url, work);
        var isFileExists = File.Exists(downloadFilePath);

        this.SetDownloadStatus(isFileExists ? State.Completed : State.None);
    }

    private async Task<bool> DownloadFile()
    {
        try
        {
            this.SetDownloadStatus(State.InProgress);

            var url = this.DownloadUrlTextBox.Text ?? "";
            var work = this.GetWorkDirectory();
            var downloadFilePath = FileHelper.GetDownloadFilePath(url, work);

            if (File.Exists(downloadFilePath))
            {
                this.SetDownloadStatus(State.Completed);
                return true;
            }

            if (!Directory.Exists(work))
            {
                Directory.CreateDirectory(work);
            }

            var progress = new Progress<int>(percent =>
            {
                this.DownloadProgressBar.Value = percent;
                this.DownloadProgressBar.IsEnabled = true;
            });

            var factory = App.Host.Services.GetRequiredService<IHttpClientFactory>();
            var downloader = new FileDownloader(factory.CreateClient());
            await downloader.DownloadFileAsync(url, downloadFilePath, progress);

            this.SetDownloadStatus(State.Completed);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
            this.SetDownloadStatus(State.Error);
            return false;
        }
    }

    private void SetDownloadStatus(State state)
    {
        switch (state)
        {
            case State.None:
                this.DownloadStatusTextBlock.Text = "未処理";
                this.DownloadProgressBar.Value = 0;
                break;
            case State.InProgress:
                this.DownloadStatusTextBlock.Text = "処理中";
                break;
            case State.Completed:
                this.DownloadStatusTextBlock.Text = "完了";
                this.DownloadProgressBar.Value = this.DownloadProgressBar.Maximum;
                break;
            case State.Error:
                this.DownloadStatusTextBlock.Text = "エラー";
                this.DownloadProgressBar.Value = 0;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private async Task<bool> ImportFile()
    {
        try
        {
            this.SetImportStatus(State.InProgress);

            var url = this.DownloadUrlTextBox.Text ?? "";
            var work = this.GetWorkDirectory();
            var downloadFilePath = FileHelper.GetDownloadFilePath(url, work);

            // ZIPファイルからCSVファイルパスを取得
            var extractedCsvPath = await Task.Run(() => ZipExtractor.ExtractFirstCsv(downloadFilePath, work));

            // CSVインポート処理
            var importer = new PostCsvImporter();
            await Task.Run(() => importer.ImportPostCsvToDatabase(extractedCsvPath));

            // ファイル削除
            FileHelper.DeleteFileIfExists(extractedCsvPath);

            this.SetImportStatus(State.Completed);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
            this.SetImportStatus(State.Error);
            return false;
        }
    }

    private void SetImportStatus(State state)
    {
        switch (state)
        {
            case State.None:
                this.ImportStatusTextBlock.Text = "未処理";
                this.ImportProgressBar.IsIndeterminate = false;
                this.ImportProgressBar.Value = 0;
                break;
            case State.InProgress:
                this.ImportStatusTextBlock.Text = "処理中";
                this.ImportProgressBar.IsIndeterminate = true;
                this.ImportProgressBar.Value = 0;
                break;
            case State.Completed:
                this.ImportStatusTextBlock.Text = "完了";
                this.ImportProgressBar.IsIndeterminate = false;
                this.ImportProgressBar.Value = this.ImportProgressBar.Maximum;
                break;
            case State.Error:
                this.ImportStatusTextBlock.Text = "エラー";
                this.ImportProgressBar.IsIndeterminate = false;
                this.ImportProgressBar.Value = 0;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

}
