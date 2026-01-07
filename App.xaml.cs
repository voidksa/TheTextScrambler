using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms; // For NotifyIcon, ContextMenu
using System.Windows.Input; // For Key enum
using TextScrambler.Models;
using TextScrambler.Services;
using TextScrambler.Views;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace TextScrambler
{
    public partial class App : Application
    {
        private NotifyIcon _notifyIcon;
        private HotkeyService _hotkeyService;
        private EncryptionService _encryptionService;
        private InputService _inputService;
        private AppSettings _settings;
        private SettingsWindow _settingsWindow;

        private int _encryptHotkeyId;
        private int _decryptHotkeyId;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Prevent multiple instances
            var exists = System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1;
            if (exists)
            {
                MessageBox.Show("The Text Scrambler is already running.", "Text Scrambler", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }

            // Services
            _encryptionService = new EncryptionService();
            _inputService = new InputService();
            _settings = SettingsManager.Load();

            // Apply Theme
            ThemeManager.ThemeChanged += OnThemeChanged;
            ThemeManager.ApplyTheme(_settings.Theme);

            // Tray Icon
            _notifyIcon = new NotifyIcon();
            try
            {
                // Try to load custom icon
                var iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/AppIcon.png")).Stream;
                using (var bitmap = new Bitmap(iconStream))
                {
                    _notifyIcon.Icon = Icon.FromHandle(bitmap.GetHicon());
                }
            }
            catch
            {
                _notifyIcon.Icon = SystemIcons.Shield; // Fallback
            }
            _notifyIcon.Visible = true;
            _notifyIcon.Text = "The Text Scrambler";
            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Font = new Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            _notifyIcon.ContextMenuStrip.ShowImageMargin = false; // We are doing flat look
            _notifyIcon.ContextMenuStrip.Padding = new Padding(0); // Remove default padding

            // Add items with some padding in text if needed, or rely on renderer
            var settingsItem = new ToolStripMenuItem("Settings", null, (s, args) => OpenSettings());
            settingsItem.Padding = new Padding(0, 5, 0, 5); // Add vertical spacing
            settingsItem.TextAlign = ContentAlignment.MiddleLeft;

            var exitItem = new ToolStripMenuItem("Exit", null, (s, args) => Shutdown());
            exitItem.Padding = new Padding(0, 5, 0, 5);
            exitItem.TextAlign = ContentAlignment.MiddleLeft;

            _notifyIcon.ContextMenuStrip.Items.Add(settingsItem);
            // _notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator()); // Removed per user request
            _notifyIcon.ContextMenuStrip.Items.Add(exitItem);

            // Apply initial theme to ContextMenu
            bool initialDark = _settings.Theme == "Dark" || (_settings.Theme == "Auto" && IsSystemDark());
            ApplyContextMenuTheme(initialDark);

            // Hotkeys
            // Need a window handle for hotkeys. 
            // Since we don't have a main window visible, we can create a hidden one or use a helper.
            // We'll create a dummy hidden window to attach the hook.
            var helperWindow = new Window
            {
                Width = 0,
                Height = 0,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = false,
                Visibility = Visibility.Hidden
            };
            helperWindow.Show(); // Must show to get handle
            helperWindow.Hide();

            var interopHelper = new System.Windows.Interop.WindowInteropHelper(helperWindow);
            _hotkeyService = new HotkeyService(interopHelper.Handle);
            _hotkeyService.HotkeyPressed += OnHotkeyPressed;

            RegisterHotkeys();
        }

        private void OnThemeChanged(bool isDark)
        {
            ApplyContextMenuTheme(isDark);
        }

        private void ApplyContextMenuTheme(bool isDark)
        {
            if (_notifyIcon?.ContextMenuStrip == null) return;

            if (isDark)
            {
                _notifyIcon.ContextMenuStrip.Renderer = new DarkContextMenuRenderer();
                _notifyIcon.ContextMenuStrip.BackColor = Color.FromArgb(45, 45, 48);
                _notifyIcon.ContextMenuStrip.ForeColor = Color.White;
            }
            else
            {
                _notifyIcon.ContextMenuStrip.Renderer = new LightContextMenuRenderer();
                _notifyIcon.ContextMenuStrip.BackColor = Color.White;
                _notifyIcon.ContextMenuStrip.ForeColor = Color.Black;
            }
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

        private void RegisterHotkeys()
        {
            // Unregister old if any
            if (_encryptHotkeyId > 0) _hotkeyService.Unregister(_encryptHotkeyId);
            if (_decryptHotkeyId > 0) _hotkeyService.Unregister(_decryptHotkeyId);

            ModifierKeys modifier = ModifierKeys.Shift;
            if (_settings.TriggerModifier == "Ctrl") modifier = ModifierKeys.Control;
            if (_settings.TriggerModifier == "Alt") modifier = ModifierKeys.Alt;

            // Encrypt: Mod + E
            _encryptHotkeyId = _hotkeyService.Register(modifier, Key.E);

            // Decrypt: Mod + D
            _decryptHotkeyId = _hotkeyService.Register(modifier, Key.D);

            if (_encryptHotkeyId == -1 || _decryptHotkeyId == -1)
            {
                _notifyIcon.ShowBalloonTip(3000, "Error", "Could not register hotkeys. Check if they are in use.", ToolTipIcon.Error);
            }
        }

        private void OnHotkeyPressed(int id)
        {
            if (id == _encryptHotkeyId)
            {
                PerformEncryption();
            }
            else if (id == _decryptHotkeyId)
            {
                PerformDecryption();
            }
        }

        private void PerformEncryption()
        {
            // 1. Get Text
            string text = _inputService.GetSelectedText();
            if (string.IsNullOrEmpty(text))
            {
                // Maybe the copy failed or nothing was selected.
                // We can show a small warning or just ignore. 
                // But if user says "nothing happens", a tip helps.
                // _notifyIcon.ShowBalloonTip(2000, "Info", "No text selected or copy failed.", ToolTipIcon.Info);
                return;
            }

            // 2. Get PIN
            string pin = _settings.DefaultPin;
            if (string.IsNullOrEmpty(pin))
            {
                // Prompt
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var pinWin = new PinWindow();
                    if (pinWin.ShowDialog() == true)
                    {
                        pin = pinWin.Pin;
                    }
                });
            }

            if (string.IsNullOrEmpty(pin)) return;

            // 3. Encrypt
            try
            {
                string encrypted = _encryptionService.Encrypt(text, pin);

                // 4. Paste
                _inputService.PasteText(encrypted);
            }
            catch (Exception ex)
            {
                _notifyIcon.ShowBalloonTip(2000, "Error", "Encryption failed: " + ex.Message, ToolTipIcon.Error);
            }
        }

        private void PerformDecryption()
        {
            // 1. Get Text
            string text = _inputService.GetSelectedText();
            if (string.IsNullOrEmpty(text)) return;

            // Check if text looks like a valid encrypted string
            if (!_encryptionService.IsEncryptedFormat(text))
            {
                // Silently ignore invalid/non-encrypted text to avoid annoying PIN prompts
                return;
            }

            // 2. Prompt for PIN (only if no default PIN is set)
            string pin = _settings.DefaultPin;

            if (string.IsNullOrEmpty(pin))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var pinWin = new PinWindow();
                    if (pinWin.ShowDialog() == true)
                    {
                        pin = pinWin.Pin;
                    }
                });
            }

            if (string.IsNullOrEmpty(pin)) return;

            // 3. Decrypt
            string decrypted = _encryptionService.Decrypt(text, pin);

            if (decrypted != null)
            {
                // 4. Paste
                _inputService.PasteText(decrypted);
            }
            else
            {
                _notifyIcon.ShowBalloonTip(2000, "Error", "Decryption failed. Wrong PIN?", ToolTipIcon.Error);
            }
        }

        private void OpenSettings()
        {
            if (_settingsWindow == null || !_settingsWindow.IsVisible)
            {
                _settingsWindow = new SettingsWindow();
                _settingsWindow.Closed += (s, e) =>
                {
                    _settingsWindow = null;
                    _settings = SettingsManager.Load(); // Reload settings
                    RegisterHotkeys(); // Re-register with new keys
                    ThemeManager.ApplyTheme(_settings.Theme); // Ensure theme is consistent
                };
                _settingsWindow.Show();
            }
            else
            {
                _settingsWindow.Activate();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _hotkeyService?.Dispose();
            _notifyIcon?.Dispose();
            base.OnExit(e);
        }
    }
}
