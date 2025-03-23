
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AloeExtImporter;

public class FileDownloader
{
    private readonly HttpClient _httpClient;

    public FileDownloader(HttpClient httpClient)
    {
        this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// 指定されたURLからファイルをダウンロードし、ローカルに保存する
    /// </summary>
    /// <param name="url">ダウンロードするファイルのURL</param>
    /// <param name="localFilePath">保存するローカルファイルのパス</param>
    /// <param name="progress">進捗通知用 IProgress (0-100%)</param>
    public async Task DownloadFileAsync(string url, string localFilePath, IProgress<int> progress = null)
    {
        if (String.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URLが空です", nameof(url));
        }

        if (String.IsNullOrWhiteSpace(localFilePath))
        {
            throw new ArgumentException("保存先のパスが空です", nameof(localFilePath));
        }

        using var response = await this._httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
        var canReportProgress = totalBytes > 0 && progress != null;

        await using var contentStream = await response.Content.ReadAsStreamAsync();
        await using var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
        var totalRead = 0L;
        var buffer = new byte[8192];
        var isMoreToRead = true;
        var lastReportedProgress = 0;

        while (isMoreToRead)
        {
            var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
            if (read == 0)
            {
                isMoreToRead = false;
                progress?.Report(100);
                continue;
            }

            await fileStream.WriteAsync(buffer, 0, read);
            totalRead += read;

            if (canReportProgress)
            {
                var progressPercentage = (int)((totalRead * 100.0) / totalBytes);
                if (progressPercentage != lastReportedProgress)
                {
                    lastReportedProgress = progressPercentage;
                    progress?.Report(progressPercentage);
                }
            }
        }
    }
}
