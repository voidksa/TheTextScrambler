using System.Windows;

namespace TextScrambler.Views
{
    public partial class PinWindow : Window
    {
        public string Pin { get; private set; }

        public PinWindow()
        {
            InitializeComponent();
            TxtPin.Focus();
        }

        protected override void OnSourceInitialized(System.EventArgs e)
        {
            base.OnSourceInitialized(e);
            // Check current resource to guess theme, or just rely on ThemeManager if we had a state.
            // Since PinWindow is transient, we can check if background is dark.
            var bgBrush = System.Windows.Application.Current.Resources["WindowBackground"] as System.Windows.Media.SolidColorBrush;
            bool isDark = false;
            if (bgBrush != null)
            {
                // Simple brightness check: if R < 128, it's dark
                if (bgBrush.Color.R < 128) isDark = true;
            }
            TextScrambler.Services.ThemeManager.UseImmersiveDarkMode(this, isDark);
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Pin = TxtPin.Password;
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
