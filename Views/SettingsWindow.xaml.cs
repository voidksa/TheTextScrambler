using System;
using System.Windows;
using System.Windows.Controls;
using TextScrambler.Models;
using TextScrambler.Services;
using Button = System.Windows.Controls.Button;

namespace TextScrambler.Views
{
    public partial class SettingsWindow : Window
    {
        private AppSettings _settings;

        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            bool isDark = _settings.Theme == "Dark" || (_settings.Theme == "Auto" && IsSystemDark());
            ThemeManager.UseImmersiveDarkMode(this, isDark);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
            base.OnClosing(e);
        }

        private bool IsSystemDark()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        var val = key.GetValue("AppsUseLightTheme");
                        if (val != null) return (int)val == 0;
                    }
                }
            }
            catch { }
            return false;
        }

        private void LoadSettings()
        {
            _settings = SettingsManager.Load();

            // Theme
            if (_settings.Theme == "Dark") RbThemeDark.IsChecked = true;
            else if (_settings.Theme == "Light") RbThemeLight.IsChecked = true;
            else RbThemeAuto.IsChecked = true;

            // Security
            TxtDefaultPin.Password = _settings.DefaultPin;

            // System
            CbStartup.IsChecked = _settings.RunOnStartup;
        }

        private void SaveSettings()
        {
            string oldTheme = _settings.Theme;

            if (RbThemeDark.IsChecked == true) _settings.Theme = "Dark";
            else if (RbThemeLight.IsChecked == true) _settings.Theme = "Light";
            else _settings.Theme = "Auto";

            _settings.DefaultPin = TxtDefaultPin.Password;
            _settings.RunOnStartup = CbStartup.IsChecked == true;

            SettingsManager.Save(_settings);

            // Apply theme immediately if changed
            if (oldTheme != _settings.Theme)
            {
                ThemeManager.ApplyTheme(_settings.Theme);
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnGithub_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btn && btn.Tag is string url)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
                }
            }
            catch { }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            // Deprecated, but kept if needed or remove. 
            // Logic moved to BtnGithub_Click
        }
    }
}