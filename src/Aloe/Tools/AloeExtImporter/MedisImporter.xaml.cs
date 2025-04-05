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
/// MedisImporter.xaml の相互作用ロジック
/// </summary>
public partial class MedisImporter : UserControl
{
    public MedisImporter()
    {
        this.InitializeComponent();
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

    private void OpenMedisButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var work = this.GetWorkDirectory();
            var filePath = FileHelper.PickFile(work);
            if (!String.IsNullOrWhiteSpace(filePath)
                && File.Exists(filePath))
            {
                this.MedisTextBox.Text = filePath;
                this.SetMedisDownloadStatus(State.Completed);
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
            this.SetMedisImportStatus(State.None);

            await this.ImportMedis();
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

    private void SetMedisDownloadStatus(State state)
    {
        switch (state)
        {
            case State.None:
                this.MedisDownloadStatusTextBlock.Text = "未処理";
                break;
            case State.InProgress:
                this.MedisDownloadStatusTextBlock.Text = "処理中";
                break;
            case State.Completed:
                this.MedisDownloadStatusTextBlock.Text = "完了";
                break;
            case State.Error:
                this.MedisDownloadStatusTextBlock.Text = "エラー";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private async Task<bool> ImportMedis()
    {
        try
        {
            this.SetMedisImportStatus(State.InProgress);

            var medisFilePath = this.MedisTextBox.Text ?? "";

            if (!File.Exists(medisFilePath))
            {
                this.SetMedisImportStatus(State.Error);
                return false;
            }

            var importer = new MedisExcelImporter();
            await Task.Run(() => importer.ImportExelToDatabase(medisFilePath));

            this.SetMedisImportStatus(State.Completed);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
            this.SetMedisImportStatus(State.Error);
            return false;
        }
    }

    private void SetMedisImportStatus(State state)
    {
        switch (state)
        {
            case State.None:
                this.MedisImportStatusTextBlock.Text = "未処理";
                this.MedisImportProgressBar.IsIndeterminate = false;
                this.MedisImportProgressBar.Value = 0;
                break;
            case State.InProgress:
                this.MedisImportStatusTextBlock.Text = "処理中";
                this.MedisImportProgressBar.IsIndeterminate = true;
                this.MedisImportProgressBar.Value = 0;
                break;
            case State.Completed:
                this.MedisImportStatusTextBlock.Text = "完了";
                this.MedisImportProgressBar.IsIndeterminate = false;
                this.MedisImportProgressBar.Value = this.MedisImportProgressBar.Maximum;
                break;
            case State.Error:
                this.MedisImportStatusTextBlock.Text = "エラー";
                this.MedisImportProgressBar.IsIndeterminate = false;
                this.MedisImportProgressBar.Value = 0;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

}
