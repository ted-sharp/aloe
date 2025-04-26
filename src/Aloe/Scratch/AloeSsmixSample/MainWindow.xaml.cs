using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using AloeSsmixSample.Data;
using AloeSsmixSample.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Ookii.Dialogs.Wpf;

namespace AloeSsmixSample;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
    }

    private void OpenSsmixPathButton_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new VistaFolderBrowserDialog
        {
            Description = "SS-MIXフォルダを選択してください",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false,
        };

        var initialPath = this.SsmixPathTextBox.Text;
        if (Directory.Exists(initialPath))
        {
            dialog.SelectedPath = initialPath;
        }

        if (dialog.ShowDialog() == true)
        {
            var selectedPath = dialog.SelectedPath;
            selectedPath += Path.DirectorySeparatorChar;
            this.SsmixPathTextBox.Text = selectedPath;
        }
    }

    private void VectorizeButton_OnClick(object sender, RoutedEventArgs e)
    {
        try
        {
            this.VectorizeButton.IsEnabled = false;
            this.VectorizeButton.Content = "解析中";

            var karteNumber = this.KarteNumberTextBox.Text.Trim();
            var ssmixRoot = this.SsmixPathTextBox.Text.Trim();

            // TODO: karteNumber から ptId を検索する
            var ptId = 0;

            var path = this.GetSsmixPatientPath(karteNumber, ssmixRoot);
            var dir = new DirectoryInfo(path);

            // ひとまずSOAP情報のXMLのみ探す。将来的にはHL7オーダー情報も含めてよい。
            var files = dir.GetFiles("*.xml");

            // TODO: そのディレクトリ内のファイルを読み取ってベクトル化する
            foreach (var file in files)
            {
                var sources = this.CreateSsmixSources(ptId, file);
                foreach (var source in sources)
                {
                    // TODO: ベクトル化したものは PostgreSQL pg_vectorに格納する
                }
            }

            this.UpdateMessageText("# SSMIXの内容をベクトル化しました。");
        }
        catch (Exception ex)
        {
            this.AppendLogText("Error: " + ex.Message);
        }
        finally
        {
            this.VectorizeButton.Content = "解析";
            this.VectorizeButton.IsEnabled = true;
        }
    }

    private string GetSsmixPatientPath(string karteNumber, string ssmixRoot)
    {

        if (karteNumber.Length < 6)
        {
            karteNumber = karteNumber.PadLeft(6, '0');
        }
        var part1 = karteNumber.Substring(0, 3);
        var part2 = karteNumber.Substring(3, 3);

        // もしssmixRootに既に患者フォルダが含まれている場合を検出する
        // 末尾の3階層が part1, part2, normalizedKarteNumber と一致していれば、
        // ユーザーが既に患者フォルダまで指定していると判断し、そのまま返す。
        var trimChars = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        var trimmedRoot = ssmixRoot.TrimEnd(trimChars);
        var segments = trimmedRoot.Split(trimChars, StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length >= 3)
        {
            var segPart1 = segments[^3];
            var segPart2 = segments[^2];
            var segFull = segments[^1];

            if (segPart1.Equals(part1, StringComparison.OrdinalIgnoreCase) &&
                segPart2.Equals(part2, StringComparison.OrdinalIgnoreCase) &&
                segFull.Equals(karteNumber, StringComparison.OrdinalIgnoreCase))
            {
                // 患者フォルダまで既に含まれているので、そのまま返す
                return trimmedRoot;
            }
        }

        return Path.Combine(ssmixRoot, part1, part2, karteNumber);
    }

    private static readonly ISet<string> SoapSectionCodes = new HashSet<string>
    {
        "MD0018560", // 主観的所見情報 (Ｓ)
        "MD0018650", // 客観的所見情報 (Ｏ)
        "MD0018830", // アセスメント情報 (Ａ)
        "MD0019420", // 計画指示情報 (Ｐ)
        "MD0022640", // 診療要約（自由記載）(Ｆ)
    };


    private SsmixSourceDto[] CreateSsmixSources(int ptId, FileInfo file)
    {
        if (!file.Extension.Equals(".xml", StringComparison.OrdinalIgnoreCase) ||
            !Hl7CdaHelper.IsSoapCda(file.FullName))
        {
            return [];
        }

        // TODO: そのうち添付ファイルも考慮したい
        var sections = Hl7CdaHelper.ExtractSections(file.FullName);
        var sources = sections
            .Where(x => SoapSectionCodes.Contains(x.SectionType))
            .Select(x => new SsmixSourceDto()
            {
                SourceId = Guid.CreateVersion7(),
                PtId = ptId,
                SourceFile = file.FullName,
                SourceKey = x.SectionKey,
                SectionType = x.SectionType,
                Content = x.Content,
                ContentHash = x.ContentHash,
            });

        return sources.ToArray();
    }

    private void UpdateMessageText(string text)
    {
        this.Dispatcher.Invoke(() =>
        {
            Debug.WriteLine(text);
            this.MessageTextBox.Text = text;
        });
    }

    private void AppendLogText(string text)
    {
        this.Dispatcher.Invoke(() =>
        {
            Debug.WriteLine(text);
            this.LogTextBox.AppendText(text);
            this.LogTextBox.AppendText(Environment.NewLine);
            this.LogTextBox.ScrollToEnd();
        });
    }

    private void MakeAdmissionSummary_OnClick(object sender, RoutedEventArgs e)
    {


    }

    private void MakeDepartmentTransferSummary_OnClick(object sender, RoutedEventArgs e)
    {


    }

    private void MakeWardTransferSummary_OnClick(object sender, RoutedEventArgs e)
    {


    }

    private void MakeDischargeSummary_OnClick(object sender, RoutedEventArgs e)
    {


    }


    #region SemanticKernel

    private Kernel? _kernel;

    private record struct SoapMessage(
        string Subjective,
        string Objective,
        string Assessment,
        string Plan);


    private async Task<SoapMessage> StartSemanticKernel(SoapMessage soap, string input)
    {
        //this._kernel ??= this.CreateKernelWithLlStudio();
        this._kernel ??= this.CreateKernelWithOpenAi();

        var responseFormat = OpenAI.Chat.ChatResponseFormat.CreateJsonSchemaFormat(
            jsonSchemaFormatName: "soap_result",
            jsonSchema: BinaryData.FromString(
                """
                    {
                        "type": "object",
                        "properties": {
                            "Subjective": { "type": "string" },
                            "Objective": { "type": "string" },
                            "Assessment": { "type": "string" },
                            "Plan": { "type": "string" },
                            "Focus": { "type": "string" },
                            "Data": { "type": "string" },
                            "Action": { "type": "string" },
                            "Response": { "type": "string" }
                        },
                        "required": ["Subjective", "Objective", "Assessment", "Plan", "Focus", "Data", "Action", "Response"],
                        "additionalProperties": false
                    }
                    """),
            jsonSchemaIsStrict: true);

        // Specify response format by setting ChatResponseFormat object in prompt execution settings.
#pragma warning disable SKEXP0010
        var settings = new OpenAIPromptExecutionSettings
        {
            ResponseFormat = responseFormat,
        };
#pragma warning restore SKEXP0010

        var json = JsonSerializer.Serialize(soap, new JsonSerializerOptions { WriteIndented = true });

        // メッセージ全体を生文字列リテラルで組み立てる
        var message =
            $"""
                ステップバイステップで考えて、電カルのSOAP/FDARメッセージを作成してください。
                入力内容は音声認識を行ったものなので、誤認識がある場合は正しく修正します。
                人間が読みやすいよう適度に改行します。
                電カルのSOAP/FDARとして使えるクオリティを目指します。
                出来上がった内容は見直しをしてください。

                # 今回入力があった文字列
                {input}
                """;

        // Send a request and pass prompt execution settings with desired response format.
        var result = await this._kernel.InvokePromptAsync(message, new(settings));
        Console.WriteLine(result);

        var newSoap = JsonSerializer.Deserialize<SoapMessage>(result.ToString());

        return newSoap;
    }

    private Kernel CreateKernelWithOpenAi()
    {
        var config = App.Host.Services.GetRequiredService<IConfiguration>();

        var deploymentName = config["AzureOpenAI:DeploymentName"]!;
        var azureEndpoint = config["AzureOpenAI:Endpoint"]!;
        var azureApiKey = config["AzureOpenAI:ApiKey"]!;
        var modelId = config["AzureOpenAI:ModelId"]!;

        var builder = Kernel.CreateBuilder();
        builder.Services.AddAzureOpenAIChatCompletion(
            deploymentName: deploymentName,
            endpoint: azureEndpoint,
            apiKey: azureApiKey,
            modelId: modelId
        );
        return builder.Build();
    }

    #endregion SemanticKernel
}
