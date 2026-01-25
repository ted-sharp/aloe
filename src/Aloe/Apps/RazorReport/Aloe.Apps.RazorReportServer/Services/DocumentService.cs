using System.Text.Json;
using System.Text.Json.Serialization;
using Aloe.Apps.RazorReportLib.Models;

namespace Aloe.Apps.RazorReportServer.Services;

/// <summary>
/// ドキュメントのシリアライズ/デシリアライズを管理するサービス
/// </summary>
public class DocumentService
{
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// コンストラクタ - JsonSerializerOptionsを初期化
    /// </summary>
    public DocumentService()
    {
        this._jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// ReportDocumentをJSON文字列にシリアライズする
    /// </summary>
    public string SerializeDocument(ReportDocument document)
    {
        return JsonSerializer.Serialize(document, this._jsonOptions);
    }

    /// <summary>
    /// JSON文字列をReportDocumentにデシリアライズする
    /// </summary>
    public ReportDocument? DeserializeDocument(string json)
    {
        return JsonSerializer.Deserialize<ReportDocument>(json, this._jsonOptions);
    }
}
