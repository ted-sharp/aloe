using System.IO;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Settings;

/// <summary>
/// ログイン画面の情報を記憶しておくためのINIファイルの定義です。
/// </summary>
public class LoginIni
{
    public string? HostUrl { get; set; }
    public bool? IsUserRemembered { get; set; }
    public bool? IsPasswordRemembered { get; set; }
    public bool? IsLoginSkipped { get; set; }
    public string? User { get; set; }
    public string? Password { get; set; }

    public bool IsReadyForAutoLogin => (this.IsLoginSkipped ?? false) &&
                                       !String.IsNullOrWhiteSpace(this.User) &&
                                       !String.IsNullOrWhiteSpace(this.Password);

    /// <summary>
    /// INIファイルを読み込みます。
    /// </summary>
    public static LoginIni Load(string filePath)
    {
        var ini = new LoginIni();
        if (!File.Exists(filePath))
        {
            return ini;
        }

        var lines = File.ReadAllLines(filePath);
        var pairs = new Dictionary<string, string>();

        foreach (var line in lines)
        {
            if (String.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var trimmedLine = line.Trim();
            if (trimmedLine.StartsWith('#') ||
                trimmedLine.StartsWith("//") ||
                trimmedLine.StartsWith("--") ||
                trimmedLine.StartsWith('[') ||
                trimmedLine.StartsWith(';'))
            {
                continue;
            }

            var parts = line.Split('=', 2);
            if (parts.Length == 2)
            {
                pairs[parts[0].Trim()] = parts[1].Trim();
            }
        }

        if (pairs.TryGetValue("HostUrl", out var hostUrl))
        {
            ini.HostUrl = hostUrl;
        }
        if (pairs.TryGetValue("IsUserRemembered", out var isUserRemembered))
        {
            ini.IsUserRemembered = Boolean.TryParse(isUserRemembered, out var result) && result;
        }
        if (pairs.TryGetValue("IsPasswordRemembered", out var isPasswordRemembered))
        {
            ini.IsPasswordRemembered = Boolean.TryParse(isPasswordRemembered, out var result) && result;
        }
        if (pairs.TryGetValue("IsLoginSkipped", out var isLoginSkipped))
        {
            ini.IsLoginSkipped = Boolean.TryParse(isLoginSkipped, out var result) && result;
        }
        if (pairs.TryGetValue("User", out var user))
        {
            ini.User = user;
        }
        if (pairs.TryGetValue("Password", out var password))
        {
            ini.Password = password;
        }

        return ini;
    }

    /// <summary>
    /// クリアします。
    /// </summary>
    public void Clear()
    {
        this.HostUrl = "";
        this.IsUserRemembered = false;
        this.IsPasswordRemembered = false;
        this.IsLoginSkipped = false;
        this.User = "";
        this.Password = "";
    }

    /// <summary>
    /// LoginIniオブジェクトをINIファイルに保存します。
    /// nullのプロパティは保存しません。
    /// </summary>
    public void Save(string filePath)
    {
        var lines = new List<string>();

        if (!String.IsNullOrWhiteSpace(this.HostUrl))
        {
            lines.Add($"HostUrl={this.HostUrl}");
        }
        if (this.IsUserRemembered.HasValue)
        {
            lines.Add($"IsUserRemembered={this.IsUserRemembered.Value}");
        }
        if (this.IsPasswordRemembered.HasValue)
        {
            lines.Add($"IsPasswordRemembered={this.IsPasswordRemembered.Value}");
        }
        if (this.IsLoginSkipped.HasValue)
        {
            lines.Add($"IsLoginSkipped={this.IsLoginSkipped.Value}");
        }
        if (!String.IsNullOrWhiteSpace(this.User))
        {
            lines.Add($"User={this.User}");
        }
        if (!String.IsNullOrWhiteSpace(this.Password))
        {
            lines.Add($"Password={this.Password}");
        }

        var directoryPath = Path.GetDirectoryName(filePath);
        if (!String.IsNullOrWhiteSpace(directoryPath) &&
            !Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        File.WriteAllLines(filePath, lines);
    }
}
