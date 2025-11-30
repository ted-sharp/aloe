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
/// MedisExamImporter.xaml の相互作用ロジック
/// </summary>
public partial class MedisExamImporter : UserControl
{
    public MedisExamImporter()
    {
        this.InitializeComponent();
    }

    private void MedisExamImporter_OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            this.RefreshMedisStatus();
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

    private void OpenMedisExamButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var work = this.GetWorkDirectory();
            var filePath = FileHelper.PickFile(work);
            if (!String.IsNullOrWhiteSpace(filePath)
                && File.Exists(filePath))
            {
                this.MedisTextBox.Text = filePath;
                this.SetMedisExamDownloadStatus(State.Completed);
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
            this.SetMedisExamImportStatus(State.None);

            await this.ImportMedisExam();
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

    private void RefreshMedisStatus()
    {
        var filePath = this.GetMedisFilePath();

        var isFileExists = File.Exists(filePath);
        if (isFileExists)
        {
            this.MedisTextBox.Text = filePath;
        }

        this.SetMedisExamDownloadStatus(isFileExists ? State.Completed : State.None);
    }


    private string GetMedisFilePath()
    {
        var work = this.GetWorkDirectory();
        var files = PathHelper.GetFiles(work, "まとめ表V*.xlsx");
        if (files.Length > 0)
        {
            return files[0];
        }
        return "";
    }

    private void SetMedisExamDownloadStatus(State state)
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

    private async Task<bool> ImportMedisExam()
    {
        try
        {
            this.SetMedisExamImportStatus(State.InProgress);

            var medisFilePath = this.MedisTextBox.Text ?? "";

            if (!File.Exists(medisFilePath))
            {
                this.SetMedisExamImportStatus(State.Error);
                return false;
            }

            var importer = new MedisImporter();
            await Task.Run(() => importer.ImportExamExelToDatabase(medisFilePath));

            this.SetMedisExamImportStatus(State.Completed);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error: " + ex.Message);
            this.SetMedisExamImportStatus(State.Error);
            return false;
        }
    }

    private void SetMedisExamImportStatus(State state)
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
