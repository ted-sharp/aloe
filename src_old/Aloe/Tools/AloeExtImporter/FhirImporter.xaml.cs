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
/// FhirImporter.xaml の相互作用ロジック
/// </summary>
public partial class FhirImporter : UserControl
{
    public FhirImporter()
    {
        this.InitializeComponent();
    }

    private void FhirImporter_OnLoaded(object sender, RoutedEventArgs e)
    {
        this.RefreshFhirDownloadStatus();
        this.RefreshFhir2DownloadStatus();
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

    private async void ImportButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            this.SetFhirDownloadStatus(State.None);
            this.SetFhir2DownloadStatus(State.None);
            this.SetFhirImportStatus(State.None);

            await this.DownloadFhirFile();
            await this.DownloadFhir2File();
            await this.ImportFhir();
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

    private void RefreshFhirDownloadStatus()
    {
        var url = this.FhirTextBox.Text ?? "";
        var work = this.GetWorkDirectory();
        var downloadFilePath = FileHelper.GetDownloadFilePath(url, work);
        var isFileExists = File.Exists(downloadFilePath);

        this.SetFhirDownloadStatus(isFileExists ? State.Completed : State.None);
    }

    private void RefreshFhir2DownloadStatus()
    {
        var url = this.Fhir2TextBox.Text ?? "";
        var work = this.GetWorkDirectory();
        var downloadFilePath = FileHelper.GetDownloadFilePath(url, work);
        var isFileExists = File.Exists(downloadFilePath);

        this.SetFhir2DownloadStatus(isFileExists ? State.Completed : State.None);
    }

    private async Task<bool> DownloadFhirFile()
    {
        try
        {
            this.SetFhirDownloadStatus(State.InProgress);

            var url = this.FhirTextBox.Text ?? "";
            var work = this.GetWorkDirectory();
            var downloadFilePath = FileHelper.GetDownloadFilePath(url, work);

            if (File.Exists(downloadFilePath))
            {
                this.SetFhirDownloadStatus(State.Completed);
                return true;
            }

            if (!Directory.Exists(work))
            {
                Directory.CreateDirectory(work);
            }

            var progress = new Progress<int>(percent =>
            {
                this.FhirDownloadProgressBar.Value = percent;
                this.FhirDownloadProgressBar.IsEnabled = true;
            });

            var factory = App.Host.Services.GetRequiredService<IHttpClientFactory>();
            var downloader = new FileDownloader(factory.CreateClient());
            await downloader.DownloadFileAsync(url, downloadFilePath, progress);

            this.SetFhirDownloadStatus(State.Completed);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
            this.SetFhirDownloadStatus(State.Error);
            return false;
        }
    }

    private async Task<bool> DownloadFhir2File()
    {
        try
        {
            this.SetFhir2DownloadStatus(State.InProgress);

            var url = this.Fhir2TextBox.Text ?? "";
            var work = this.GetWorkDirectory();
            var downloadFilePath = FileHelper.GetDownloadFilePath(url, work);

            if (File.Exists(downloadFilePath))
            {
                this.SetFhir2DownloadStatus(State.Completed);
                return true;
            }

            if (!Directory.Exists(work))
            {
                Directory.CreateDirectory(work);
            }

            var progress = new Progress<int>(percent =>
            {
                this.Fhir2DownloadProgressBar.Value = percent;
                this.Fhir2DownloadProgressBar.IsEnabled = true;
            });

            var factory = App.Host.Services.GetRequiredService<IHttpClientFactory>();
            var downloader = new FileDownloader(factory.CreateClient());
            await downloader.DownloadFileAsync(url, downloadFilePath, progress);

            this.SetFhir2DownloadStatus(State.Completed);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
            this.SetFhir2DownloadStatus(State.Error);
            return false;
        }
    }

    private void SetFhirDownloadStatus(State state)
    {
        switch (state)
        {
            case State.None:
                this.FhirDownloadStatusTextBlock.Text = "未処理";
                this.FhirDownloadProgressBar.Value = 0;
                break;
            case State.InProgress:
                this.FhirDownloadStatusTextBlock.Text = "処理中";
                break;
            case State.Completed:
                this.FhirDownloadStatusTextBlock.Text = "完了";
                this.FhirDownloadProgressBar.Value = this.FhirDownloadProgressBar.Maximum;
                break;
            case State.Error:
                this.FhirDownloadStatusTextBlock.Text = "エラー";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private void SetFhir2DownloadStatus(State state)
    {
        switch (state)
        {
            case State.None:
                this.Fhir2DownloadStatusTextBlock.Text = "未処理";
                this.Fhir2DownloadProgressBar.Value = 0;
                break;
            case State.InProgress:
                this.Fhir2DownloadStatusTextBlock.Text = "処理中";
                break;
            case State.Completed:
                this.Fhir2DownloadStatusTextBlock.Text = "完了";
                this.Fhir2DownloadProgressBar.Value = this.Fhir2DownloadProgressBar.Maximum;
                break;
            case State.Error:
                this.Fhir2DownloadStatusTextBlock.Text = "エラー";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private async Task<bool> ImportFhir()
    {
        try
        {
            this.SetFhirImportStatus(State.InProgress);

            var url = this.FhirTextBox.Text ?? "";
            var work = this.GetWorkDirectory();
            var downloadFilePath = FileHelper.GetDownloadFilePath(url, work);

            var url2 = this.Fhir2TextBox.Text ?? "";
            var downloadFilePath2 = FileHelper.GetDownloadFilePath(url2, work);

            if (!(File.Exists(downloadFilePath) && File.Exists(downloadFilePath2)))
            {
                this.SetFhirImportStatus(State.Error);
                return false;
            }

            var importer = new FhirJsonImporter();
            await Task.Run(async () =>
            {
                await importer.TruncateDatabase();
                await importer.ImportJsonToDatabase(downloadFilePath);
                await importer.ImportJsonToDatabase(downloadFilePath2);
            });

            this.SetFhirImportStatus(State.Completed);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
            this.SetFhirImportStatus(State.Error);
            return false;
        }
    }

    private void SetFhirImportStatus(State state)
    {
        switch (state)
        {
            case State.None:
                this.FhirImportStatusTextBlock.Text = "未処理";
                this.FhirImportProgressBar.IsIndeterminate = false;
                this.FhirImportProgressBar.Value = 0;
                break;
            case State.InProgress:
                this.FhirImportStatusTextBlock.Text = "処理中";
                this.FhirImportProgressBar.IsIndeterminate = true;
                this.FhirImportProgressBar.Value = 0;
                break;
            case State.Completed:
                this.FhirImportStatusTextBlock.Text = "完了";
                this.FhirImportProgressBar.IsIndeterminate = false;
                this.FhirImportProgressBar.Value = this.FhirImportProgressBar.Maximum;
                break;
            case State.Error:
                this.FhirImportStatusTextBlock.Text = "エラー";
                this.FhirImportProgressBar.IsIndeterminate = false;
                this.FhirImportProgressBar.Value = 0;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }
}
