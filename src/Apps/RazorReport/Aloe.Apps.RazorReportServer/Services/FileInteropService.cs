using Microsoft.JSInterop;

namespace Aloe.Apps.RazorReportServer.Services;

/// <summary>
/// ブラウザのファイル操作をサポートするサービス
/// </summary>
public class FileInteropService
{
    private readonly IJSRuntime _jsRuntime;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public FileInteropService(IJSRuntime jsRuntime)
    {
        this._jsRuntime = jsRuntime;
    }

    /// <summary>
    /// ファイルをダウンロードする
    /// </summary>
    public async Task DownloadFileAsync(string filename, string content, string mimeType = "application/json")
    {
        try
        {
            var module = await this._jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./js/fileInterop.js");
            await module.InvokeVoidAsync("downloadFile", filename, content, mimeType);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("ファイルのダウンロードに失敗しました。", ex);
        }
    }
}
