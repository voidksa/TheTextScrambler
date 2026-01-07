using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Application = System.Windows.Application;

namespace TextScrambler.Services
{
    public static class ThemeManager
    {
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string RegistryValueName = "AppsUseLightTheme";

        public static event Action<bool> ThemeChanged;

        public static void ApplyTheme(string themeMode)
        {
            if (themeMode == "Auto")
            {
                ApplySystemTheme();
            }
            else if (themeMode == "Dark")
            {
                ApplyDarkTheme();
            }
            else
            {
                ApplyLightTheme();
            }
        }

        private static void ApplySystemTheme()
        {
            bool isLight = true;
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
                {
                    if (key != null)
                    {
                        object registryValueObject = key.GetValue(RegistryValueName);
                        if (registryValueObject != null)
                        {
                            int registryValue = (int)registryValueObject;
                            isLight = registryValue > 0;
                        }
                    }
                }
            }
            catch
            {
                // Default to light if fails
            }

            if (isLight) ApplyLightTheme();
            else ApplyDarkTheme();
        }

        private static void ApplyDarkTheme()
        {
            var dict = new ResourceDictionary();
            // Core Colors (Modern Fluent Dark)
            dict["WindowBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#202020"));
            dict["TextForeground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
            dict["ControlBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D2D2D"));
            dict["BorderBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#454545"));
            dict["HeaderForeground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
            dict["SecondaryText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A0A0A0"));

            // Interactive Colors
            dict["ButtonBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#333333"));
            dict["ButtonHover"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3F3F3F"));
            dict["ButtonPressed"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#282828"));
            dict["InputBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2B2B2B"));

            // Accent Color (Fluent Blue)
            dict["AccentColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#60CDFF"));
            dict["AccentForeground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000")); // Black text on bright blue

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dict);

            UpdateWindowsTheme(true);
            ThemeChanged?.Invoke(true);
        }

        private static void ApplyLightTheme()
        {
            var dict = new ResourceDictionary();
            // Core Colors (Modern Fluent Light)
            dict["WindowBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9F9F9"));
            dict["TextForeground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F1F1F"));
            dict["ControlBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
            dict["BorderBrush"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
            dict["HeaderForeground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F1F1F"));
            dict["SecondaryText"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5D5D5D"));

            // Interactive Colors
            dict["ButtonBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
            dict["ButtonHover"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F5F5"));
            dict["ButtonPressed"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EBEBEB"));
            dict["InputBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));

            // Accent Color (Standard Windows Blue)
            dict["AccentColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0067C0"));
            dict["AccentForeground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dict);

            UpdateWindowsTheme(false);
            ThemeChanged?.Invoke(false);
        }

        private static void UpdateWindowsTheme(bool isDark)
        {
            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                UseImmersiveDarkMode(window, isDark);
            }
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public static bool UseImmersiveDarkMode(Window window, bool enabled)
        {
            if (window == null) return false;
            try
            {
                var windowInteropHelper = new System.Windows.Interop.WindowInteropHelper(window);
                var handle = windowInteropHelper.Handle;
                if (handle == IntPtr.Zero) return false; // Window not initialized yet

                int useImmersiveDarkMode = enabled ? 1 : 0;
                if (DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useImmersiveDarkMode, sizeof(int)) == 0)
                {
                    return true;
                }
                else
                {
                    // Try older attribute for older Windows 10 versions
                    if (DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref useImmersiveDarkMode, sizeof(int)) == 0)
                    {
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }
    }
}
