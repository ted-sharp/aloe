using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AloeExtImporter;

public class ZipExtractor
{
    /// <summary>
    /// ZIPファイルを解凍し、最初のCSVファイルのパスを返す
    /// </summary>
    /// <param name="zipFilePath">ZIPファイルのパス</param>
    /// <param name="extractDir">解凍先ディレクトリ</param>
    /// <returns>最初のCSVファイルのフルパス</returns>
    public static string ExtractFirstCsv(string zipFilePath, string extractDir)
    {
        if (!File.Exists(zipFilePath))
        {
            throw new FileNotFoundException($"ZIPファイルが見つかりません: {zipFilePath}");
        }

        if (!Directory.Exists(extractDir))
        {
            Directory.CreateDirectory(extractDir);
        }

        // ZIP解凍（.NET 6+ なら上書き可能なオプションあり）
        ZipFile.ExtractToDirectory(zipFilePath, extractDir, true);

        // 最初に見つかったCSVファイルのパスを取得
        var csvFiles = Directory.GetFiles(extractDir, "*.csv", SearchOption.AllDirectories);
        if (csvFiles.Length > 0)
        {
            return csvFiles[0];
        }

        throw new FileNotFoundException("ZIP解凍後にCSVファイルが見つかりませんでした。");
    }

    public static string Extract(string zipFilePath, string extractDir)
    {
        if (!File.Exists(zipFilePath))
        {
            throw new FileNotFoundException($"ZIPファイルが見つかりません: {zipFilePath}");
        }

        if (!Directory.Exists(extractDir))
        {
            Directory.CreateDirectory(extractDir);
        }

        using var archive = ZipFile.OpenRead(zipFilePath);

        // 最初のファイルの共通プレフィックス（フォルダ名）を探す
        var topLevelDir = archive.Entries
            .Select(entry => Path.GetDirectoryName(entry.FullName)?.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).FirstOrDefault())
            .Where(name => !String.IsNullOrEmpty(name))
            .Distinct()
            .FirstOrDefault();

        // topLevelDirがなければzipファイルの名前を使う
        if (String.IsNullOrWhiteSpace(topLevelDir))
        {
            topLevelDir ??= Path.GetFileNameWithoutExtension(zipFilePath);
            if (!Directory.Exists(topLevelDir))
            {
                // topLevelDirがなければ作成する
                Directory.CreateDirectory(topLevelDir);
            }

            // 作成したディレクトリの中に解凍
            ZipFile.ExtractToDirectory(zipFilePath, topLevelDir, true);
        }
        else
        {
            // ZIP解凍
            ZipFile.ExtractToDirectory(zipFilePath, extractDir, true);
        }

        return topLevelDir;
    }
}
