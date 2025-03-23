
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Navigation;


namespace AloeExtImporter;

public static class FileHelper
{
    /// <summary>
    /// ハイパーリンクをクリックした際にデフォルトブラウザで開く
    /// </summary>
    public static void OpenLink(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"リンクを開くことができませんでした: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// フォルダ選択ダイアログを表示し、選択されたフォルダのパスを返す
    /// </summary>
    public static string? PickFolder(string currentDirectoryPath)
    {
        using var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "作業フォルダを選択してください",
            UseDescriptionForTitle = true,
        };

        var fullPath = Path.GetFullPath(currentDirectoryPath);
        if (Directory.Exists(fullPath))
        {
            dialog.InitialDirectory = fullPath;
        }

        var isPicked = dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK;
        if (!isPicked)
        {
            return null;
        }

        var selectedPath = dialog.SelectedPath;

        try
        {
            // 現在の作業ディレクトリを基準に相対パスに変換
            var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), selectedPath);
            return relativePath;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to convert to relative path: {ex.Message}");
            return selectedPath; // 失敗時はフルパスを返す
        }

    }

    /// <summary>
    /// フォルダ選択ダイアログを表示し、選択されたフォルダのパスを返す
    /// </summary>
    public static string? PickFile(string currentDirectoryPath)
    {
        using var dialog = new System.Windows.Forms.OpenFileDialog
        {
            Title = "ファイルを選択してください",
            CheckFileExists = true,
            Multiselect = false,
        };

        var fullPath = Path.GetFullPath(currentDirectoryPath);
        if (Directory.Exists(fullPath))
        {
            dialog.InitialDirectory = fullPath;
        }

        var isPicked = dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK;
        if (!isPicked)
        {
            return null;
        }

        var selectedPath = dialog.FileName;

        try
        {
            // 現在の作業ディレクトリを基準に相対パスに変換
            var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), selectedPath);
            return relativePath;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to convert to relative path: {ex.Message}");
            return selectedPath; // 失敗時はフルパスを返す
        }

    }

    /// <summary>
    /// URLからファイル名を取得し、指定した作業ディレクトリに結合してフルパスを返す
    /// </summary>
    public static string GetDownloadFilePath(string url, string workDir)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url, nameof(url));
        ArgumentException.ThrowIfNullOrWhiteSpace(workDir, nameof(workDir));

        var uri = new Uri(url);
        var fileName = Path.GetFileName(uri.LocalPath);
        if (String.IsNullOrEmpty(fileName))
        {
            fileName = "downloaded_file";
        }

        return Path.Combine(workDir, fileName);
    }

    /// <summary>
    /// 指定したファイルが存在する場合、削除する
    /// </summary>
    public static void DeleteFileIfExists(string filePath)
    {
        if (String.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.WriteLine($"Deleted file: {filePath}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to delete file: {filePath}, Error: {ex.Message}");
        }
    }
}
