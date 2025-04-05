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
/// MhlwImporter.xaml の相互作用ロジック
/// </summary>
public partial class MhlwImporter : UserControl
{
    public MhlwImporter()
    {
        this.InitializeComponent();
    }

    private void MhlwImporter_OnLoaded(object sender, RoutedEventArgs e)
    {
        this.RefreshMhlwDownloadStatus();
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

    private void OpenMhlwButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var work = this.GetWorkDirectory();
            var filePath = FileHelper.PickFile(work);
            if (!String.IsNullOrWhiteSpace(filePath)
                && File.Exists(filePath))
            {
                this.MhlwTextBox.Text = filePath;
                this.SetMhlwDownloadStatus(State.Completed);
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
            this.SetMhlwImportStatus(State.None);

            await this.ImportMhlw();
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

    private void RefreshMhlwDownloadStatus()
    {
        var filePath = this.MhlwTextBox.Text;
        var isFileExists = File.Exists(filePath);

        this.SetMhlwDownloadStatus(isFileExists ? State.Completed : State.None);
    }

    private void SetMhlwDownloadStatus(State state)
    {
        switch (state)
        {
            case State.None:
                this.MhlwDownloadStatusTextBlock.Text = "未処理";
                break;
            case State.InProgress:
                this.MhlwDownloadStatusTextBlock.Text = "処理中";
                break;
            case State.Completed:
                this.MhlwDownloadStatusTextBlock.Text = "完了";
                break;
            case State.Error:
                this.MhlwDownloadStatusTextBlock.Text = "エラー";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private async Task<bool> ImportMhlw()
    {
        try
        {
            this.SetMhlwImportStatus(State.InProgress);

            var medisFilePath = this.MhlwTextBox.Text ?? "";

            if (!File.Exists(medisFilePath))
            {
                this.SetMhlwImportStatus(State.Error);
                return false;
            }

            var importer = new MhlwExcelImporter();
            await Task.Run(() => importer.ImportExelToDatabase(medisFilePath));

            this.SetMhlwImportStatus(State.Completed);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
            this.SetMhlwImportStatus(State.Error);
            return false;
        }
    }

    private void SetMhlwImportStatus(State state)
    {
        switch (state)
        {
            case State.None:
                this.MhlwImportStatusTextBlock.Text = "未処理";
                this.MhlwImportProgressBar.IsIndeterminate = false;
                this.MhlwImportProgressBar.Value = 0;
                break;
            case State.InProgress:
                this.MhlwImportStatusTextBlock.Text = "処理中";
                this.MhlwImportProgressBar.IsIndeterminate = true;
                this.MhlwImportProgressBar.Value = 0;
                break;
            case State.Completed:
                this.MhlwImportStatusTextBlock.Text = "完了";
                this.MhlwImportProgressBar.IsIndeterminate = false;
                this.MhlwImportProgressBar.Value = this.MhlwImportProgressBar.Maximum;
                break;
            case State.Error:
                this.MhlwImportStatusTextBlock.Text = "エラー";
                this.MhlwImportProgressBar.IsIndeterminate = false;
                this.MhlwImportProgressBar.Value = 0;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }
}
