using System;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace TextScrambler.Services
{
    public class HotkeyService : IDisposable
    {
        private const int WM_HOTKEY = 0x0312;
        private HwndSource _source;
        private nint _windowHandle;
        private int _currentId;

        public event Action<int> HotkeyPressed;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public HotkeyService(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);
            _currentId = 9000; // Start ID
        }

        public int Register(ModifierKeys modifiers, Key key)
        {
            _currentId++;
            uint vk = (uint)KeyInterop.VirtualKeyFromKey(key);
            uint fsModifiers = (uint)modifiers;
            
            if (!RegisterHotKey(_windowHandle, _currentId, fsModifiers, vk))
            {
                // Failed
                // throw new InvalidOperationException("Could not register hotkey.");
                return -1; 
            }
            return _currentId;
        }

        public void Unregister(int id)
        {
            UnregisterHotKey(_windowHandle, id);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                HotkeyPressed?.Invoke(id);
                handled = true;
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            _source?.RemoveHook(HwndHook);
            // Unregister all handled by individual calls or tracking list
        }
    }
}
