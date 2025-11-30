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
using Aloe.Common.AloeCoreLib.Util;

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
        try
        {
            this.RefreshHoujinDownloadStatus();
            this.RefreshJisDownloadStatus();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
        }
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

    private void OpenHoujinButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var work = this.GetWorkDirectory();
            var filePath = FileHelper.PickFile(work);
            if (!String.IsNullOrWhiteSpace(filePath)
                && File.Exists(filePath))
            {
                this.HoujinTextBox.Text = filePath;
                this.SetHoujinDownloadStatus(State.Completed);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
        }
    }

    private async void ImportButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            this.SetJisDownloadStatus(State.None);
            this.SetJisImportStatus(State.None);
            this.SetHoujinImportStatus(State.None);

            await this.DownloadJis();
            await this.ImportJis();
            await this.ImportHoujin();
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

    private void RefreshHoujinDownloadStatus()
    {
        var filePath = this.GetHoujinCsvFilePath();

        var isFileExists = File.Exists(filePath);
        if (isFileExists)
        {
            this.HoujinTextBox.Text = filePath;
        }

        this.SetHoujinDownloadStatus(isFileExists ? State.Completed : State.None);
    }

    private string GetHoujinCsvFilePath()
    {
        var work = this.GetWorkDirectory();
        var files = PathHelper.GetFiles(work, "00_zenkoku_all_*.zip");
        if (files.Length > 0)
        {
            return files[0];
        }

        return "";
    }

    private void SetHoujinDownloadStatus(State state)
    {
        switch (state)
        {
            case State.None:
                this.HoujinDownloadStatusTextBlock.Text = "未処理";
                break;
            case State.InProgress:
                this.HoujinDownloadStatusTextBlock.Text = "処理中";
                break;
            case State.Completed:
                this.HoujinDownloadStatusTextBlock.Text = "完了";
                break;
            case State.Error:
                this.HoujinDownloadStatusTextBlock.Text = "エラー";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private void RefreshJisDownloadStatus()
    {
        var url = this.JisDownloadUrlTextBox.Text ?? "";
        var work = this.GetWorkDirectory();
        var downloadFilePath = FileHelper.GetDownloadFilePath(url, work);
        var isFileExists = File.Exists(downloadFilePath);

        this.SetJisDownloadStatus(isFileExists ? State.Completed : State.None);
    }

    private async Task<bool> DownloadJis()
    {
        try
        {
            this.SetJisDownloadStatus(State.InProgress);

            var url = this.JisDownloadUrlTextBox.Text ?? "";
            var work = this.GetWorkDirectory();
            var downloadFilePath = FileHelper.GetDownloadFilePath(url, work);

            if (File.Exists(downloadFilePath))
            {
                this.SetJisDownloadStatus(State.Completed);
                return true;
            }

            if (!Directory.Exists(work))
            {
                Directory.CreateDirectory(work);
            }

            var progress = new Progress<int>(percent =>
            {
                this.JisDownloadProgressBar.Value = percent;
                this.JisDownloadProgressBar.IsEnabled = true;
            });

            var factory = App.Host.Services.GetRequiredService<IHttpClientFactory>();
            var downloader = new FileDownloader(factory.CreateClient());
            await downloader.DownloadFileAsync(url, downloadFilePath, progress);

            this.SetJisDownloadStatus(State.Completed);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
            this.SetJisDownloadStatus(State.Error);
            return false;
        }
    }

    private void SetJisDownloadStatus(State state)
    {
        switch (state)
        {
            case State.None:
                this.JisDownloadStatusTextBlock.Text = "未処理";
                this.JisDownloadProgressBar.Value = 0;
                break;
            case State.InProgress:
                this.JisDownloadStatusTextBlock.Text = "処理中";
                break;
            case State.Completed:
                this.JisDownloadStatusTextBlock.Text = "完了";
                this.JisDownloadProgressBar.Value = this.JisDownloadProgressBar.Maximum;
                break;
            case State.Error:
                this.JisDownloadStatusTextBlock.Text = "エラー";
                this.JisDownloadProgressBar.Value = 0;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private async Task<bool> ImportJis()
    {
        try
        {
            this.SetJisImportStatus(State.InProgress);

            var url = this.JisDownloadUrlTextBox.Text ?? "";
            var work = this.GetWorkDirectory();
            var jisFilePath = FileHelper.GetDownloadFilePath(url, work);

            if (!File.Exists(jisFilePath))
            {
                this.SetJisImportStatus(State.Error);
                return false;
            }

            var importer = new JisCompatMapExcelImporter();
            await Task.Run(() => importer.ImportExelToDatabase(jisFilePath));
            await Task.Run(() => importer.InsertIbmKanji());

            this.SetJisImportStatus(State.Completed);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
            this.SetJisImportStatus(State.Error);
            return false;
        }
    }

    private void SetJisImportStatus(State state)
    {
        switch (state)
        {
            case State.None:
                this.JisImportStatusTextBlock.Text = "未処理";
                this.JisImportProgressBar.IsIndeterminate = false;
                this.JisImportProgressBar.Value = 0;
                break;
            case State.InProgress:
                this.JisImportStatusTextBlock.Text = "処理中";
                this.JisImportProgressBar.IsIndeterminate = true;
                this.JisImportProgressBar.Value = 0;
                break;
            case State.Completed:
                this.JisImportStatusTextBlock.Text = "完了";
                this.JisImportProgressBar.IsIndeterminate = false;
                this.JisImportProgressBar.Value = this.JisImportProgressBar.Maximum;
                break;
            case State.Error:
                this.JisImportStatusTextBlock.Text = "エラー";
                this.JisImportProgressBar.IsIndeterminate = false;
                this.JisImportProgressBar.Value = 0;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private async Task<bool> ImportHoujin()
    {
        try
        {
            this.SetHoujinImportStatus(State.InProgress);

            var houjinFilePath = this.HoujinTextBox.Text ?? "";

            if (!File.Exists(houjinFilePath))
            {
                this.SetHoujinImportStatus(State.Error);
                return false;
            }

            var work = this.GetWorkDirectory();

            // ZIPファイルからCSVファイルパスを取得
            var extractedCsvPath = await Task.Run(() => ZipExtractor.ExtractFirstCsv(houjinFilePath, work));

            // CSVインポート処理
            var importer = new HoujinCsvImporter();
            await Task.Run(() => importer.ImportHoujinCsvToDatabase(extractedCsvPath));

            // ファイル削除
            FileHelper.DeleteFileIfExists(extractedCsvPath);

            this.SetHoujinImportStatus(State.Completed);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
            this.SetHoujinImportStatus(State.Error);
            return false;
        }
    }

    private void SetHoujinImportStatus(State state)
    {
        switch (state)
        {
            case State.None:
                this.HoujinImportStatusTextBlock.Text = "未処理";
                this.HoujinImportProgressBar.IsIndeterminate = false;
                this.HoujinImportProgressBar.Value = 0;
                break;
            case State.InProgress:
                this.HoujinImportStatusTextBlock.Text = "処理中";
                this.HoujinImportProgressBar.IsIndeterminate = true;
                this.HoujinImportProgressBar.Value = 0;
                break;
            case State.Completed:
                this.HoujinImportStatusTextBlock.Text = "完了";
                this.HoujinImportProgressBar.IsIndeterminate = false;
                this.HoujinImportProgressBar.Value = this.HoujinImportProgressBar.Maximum;
                break;
            case State.Error:
                this.HoujinImportStatusTextBlock.Text = "エラー";
                this.HoujinImportProgressBar.IsIndeterminate = false;
                this.HoujinImportProgressBar.Value = 0;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

}
