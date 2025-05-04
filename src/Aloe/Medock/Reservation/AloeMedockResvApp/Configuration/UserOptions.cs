using System.IO;
using System.Text.Json;
using Aloe.Common.AloeCoreLib.Util;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Configuration;

/// <summary>
/// ログイン画面の情報を記憶しておくための定義です。
/// </summary>
public class UserOptions
{
    public static string FileName = "appsettings.useroptions.json";

    private static readonly JsonSerializerOptions s_jsonWriteOptions = new()
    {
        WriteIndented = true,
    };

    /// <summary>
    /// ホストのURLを
    /// </summary>
    public string? HostUrl { get; set; }

    public bool? IsUserRemembered { get; set; }

    public bool? IsPasswordRemembered { get; set; }

    public bool? IsLoginSkipped { get; set; }

    public string? User { get; set; }

    public string? Password { get; set; }

    public bool IsReadyForAutoLogin => (this.IsLoginSkipped ?? false) &&
                                       !String.IsNullOrWhiteSpace(this.User) &&
                                       !String.IsNullOrWhiteSpace(this.Password);

    public string ToJson()
    {
        var wrapper = new
        {
            UserOptions = this,
        };

        return JsonSerializer.Serialize(wrapper, UserOptions.s_jsonWriteOptions);
    }

    public void Save()
    {
        var fullPath = PathHelper.FromBase(UserOptions.FileName);
        var json = this.ToJson();
        File.WriteAllText(fullPath, json);
    }
}
