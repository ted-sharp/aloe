using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Aloe.Common.AloeCoreLib.Wpf.Utils;

public static class ThemeHelper
{
    public enum ThemeMode
    {
        Light,
        Dark,
        System
    }

    public static bool IsSystemInDarkMode()
    {
        try
        {
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return (int?)key?.GetValue("AppsUseLightTheme", 1) == 0;
        }
        catch
        {
            return false; // fallback to light
        }
    }
}

public static class ThemeManager
{
    public static ThemeHelper.ThemeMode CurrentMode { get; private set; } = ThemeHelper.ThemeMode.System;

    public static void ApplyTheme(ThemeHelper.ThemeMode mode)
    {
        CurrentMode = mode;

        bool isDark = mode switch
        {
            ThemeHelper.ThemeMode.Dark => true,
            ThemeHelper.ThemeMode.Light => false,
            ThemeHelper.ThemeMode.System => ThemeHelper.IsSystemInDarkMode(),
            _ => false
        };

        var dicts = Application.Current.Resources.MergedDictionaries;

        // Fluent テーマはそのまま使う（色は内部で選ばれる）
        dicts.Clear();
        dicts.Add(new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/PresentationFramework.Fluent;component/Themes/Fluent.xaml")
        });

        dicts.Add(new ResourceDictionary
        {
            Source = new Uri(isDark ? "Themes/Dark.xaml" : "Themes/Light.xaml", UriKind.Relative)
        });
    }
}
