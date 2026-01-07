using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace TextScrambler.Services
{
    public class InputService
    {
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const byte VK_CONTROL = 0x11;
        private const byte VK_SHIFT = 0x10;
        private const byte VK_C = 0x43;
        private const byte VK_V = 0x56;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        private const byte VK_MENU = 0x12; // Alt

        public string GetSelectedText()
        {
            try
            {
                try
                {
                    Clipboard.Clear();
                }
                catch { }

                // CRITICAL: Release Shift immediately before Ctrl+C to avoid "Ctrl+Shift+C"
                // if the user is physically holding the Shift key (auto-repeat).
                keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

                // Smallest safe delay to let OS process the KeyUp
                Thread.Sleep(10);

                // Press Ctrl
                keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
                // Press C
                keybd_event(VK_C, 0, 0, UIntPtr.Zero);

                // Wait a bit for the app to register the key press
                Thread.Sleep(10);

                // Release C
                keybd_event(VK_C, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                // Release Ctrl
                keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

                // Wait for clipboard
                for (int i = 0; i < 25; i++)
                {
                    try
                    {
                        if (Clipboard.ContainsText())
                        {
                            return Clipboard.GetText();
                        }
                    }
                    catch { }
                    Thread.Sleep(20);
                }

                return string.Empty;
            }
            finally
            {
                // Removed ResetKeys from here to avoid interference
            }
        }

        public void PasteText(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            try
            {
                // Set Clipboard
                bool set = false;
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        Clipboard.SetText(text);
                        set = true;
                        break;
                    }
                    catch
                    {
                        Thread.Sleep(50);
                    }
                }

                if (!set) return;

                // Ensure Shift is released
                keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                Thread.Sleep(50);

                // Send Ctrl+V
                keybd_event(VK_CONTROL, 0, 0, UIntPtr.Zero);
                keybd_event(VK_V, 0, 0, UIntPtr.Zero);
                keybd_event(VK_V, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            }
            finally
            {
                ResetKeys();
            }
        }

        private void ResetKeys()
        {
            // Force release all modifiers to prevent "stuck" keys
            keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // Alt
        }
    }
}
