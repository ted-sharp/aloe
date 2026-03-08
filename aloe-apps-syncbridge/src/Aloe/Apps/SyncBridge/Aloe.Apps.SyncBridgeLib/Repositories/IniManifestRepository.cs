using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Aloe.Apps.SyncBridgeLib.Models;

namespace Aloe.Apps.SyncBridgeLib.Repositories
{
    public class IniManifestRepository : IManifestRepository
    {
        private const string ManifestFileName = "manifest.ini";
        private const int BufferSize = 32767;

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(
            string section,
            string key,
            string defaultValue,
            StringBuilder returnValue,
            int size,
            string filePath);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileSectionNames(
            IntPtr returnValue,
            int size,
            string filePath);

        public SyncManifest LoadManifest()
        {
            string manifestPath = this.GetManifestPath();

            if (!File.Exists(manifestPath))
            {
                throw new FileNotFoundException($"マニフェストファイルが見つかりません: {manifestPath}");
            }

            var manifest = new SyncManifest
            {
                Version = this.GetValue(manifestPath, "Manifest", "Version"),
                SourceRootPath = Environment.ExpandEnvironmentVariables(this.GetValue(manifestPath, "Manifest", "SourceRootPath")),
                LocalBasePath = Environment.ExpandEnvironmentVariables(this.GetValue(manifestPath, "Manifest", "LocalBasePath")),
                Runtime = this.LoadRuntime(manifestPath),
                SyncOptions = this.LoadSyncOptions(manifestPath),
                Applications = this.LoadApplications(manifestPath)
            };

            return manifest;
        }

        private RuntimeConfig LoadRuntime(string manifestPath)
        {
            return new RuntimeConfig
            {
                RelativePath = this.GetValue(manifestPath, "Runtime", "RelativePath"),
                ZipFileName = this.GetValue(manifestPath, "Runtime", "ZipFileName", "")
            };
        }

        private SyncOptions LoadSyncOptions(string manifestPath)
        {
            var options = new SyncOptions();

            string skipPatterns = this.GetValue(manifestPath, "SyncOptions", "SkipPatterns", "");
            if (!String.IsNullOrEmpty(skipPatterns))
            {
                options.SkipPatterns = skipPatterns
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .ToList();
            }

            return options;
        }

        private List<AppConfig> LoadApplications(string manifestPath)
        {
            var apps = new List<AppConfig>();
            var sectionNames = this.GetAllSectionNames(manifestPath);

            foreach (var sectionName in sectionNames.Where(s => s.StartsWith("App.")))
            {
                string appId = sectionName.Substring(4);

                var app = new AppConfig
                {
                    AppId = appId,
                    RelativePath = this.GetValue(manifestPath, sectionName, "RelativePath"),
                    EntryDll = this.GetValue(manifestPath, sectionName, "EntryDll"),
                    LaunchArgPattern = this.GetValue(manifestPath, sectionName, "LaunchArgPattern", ""),
                    ZipFileName = this.GetValue(manifestPath, sectionName, "ZipFileName", "")
                };

                apps.Add(app);
            }

            return apps;
        }

        private List<string> GetAllSectionNames(string manifestPath)
        {
            IntPtr buffer = Marshal.AllocHGlobal(BufferSize * 2);
            try
            {
                int length = GetPrivateProfileSectionNames(buffer, BufferSize, manifestPath);
                if (length == 0)
                    return new List<string>();

                string result = Marshal.PtrToStringUni(buffer, length - 1);
                return result.Split('\0').Where(s => !String.IsNullOrEmpty(s)).ToList();
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private string GetValue(string manifestPath, string section, string key, string defaultValue = "")
        {
            var sb = new StringBuilder(BufferSize);
            GetPrivateProfileString(section, key, defaultValue, sb, BufferSize, manifestPath);
            return sb.ToString();
        }

        private string GetManifestPath()
        {
            string exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return Path.Combine(exeDir, ManifestFileName);
        }
    }
}
