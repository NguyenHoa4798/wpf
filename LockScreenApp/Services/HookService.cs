using LockScreenApp.Utilities;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace LockScreenApp.Services
{
    public class HookService : IDisposable
    {
        private IntPtr _keyboardHookId = IntPtr.Zero;
        private IntPtr _mouseHookId = IntPtr.Zero;
        private WinApiHelper.LowLevelKeyboardProc _keyboardProc;
        private WinApiHelper.LowLevelMouseProc _mouseProc;

        public HookService()
        {
            _keyboardProc = KeyboardHookCallback;
            _mouseProc = MouseHookCallback;
        }

        public void StartHooks()
        {
            _keyboardHookId = SetHook(_keyboardProc, WinApiHelper.WH_KEYBOARD_LL);
            _mouseHookId = SetHook(_mouseProc, WinApiHelper.WH_MOUSE_LL);
        }

        private IntPtr SetHook(Delegate proc, int hookType)
        {
            using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            return WinApiHelper.SetWindowsHookEx(hookType, proc, WinApiHelper.GetModuleHandle(curModule.ModuleName), 0);
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Key key = KeyInterop.KeyFromVirtualKey(vkCode);

                // Kiểm tra phím tắt bí mật: Ctrl + Shift + Q
                bool isCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
                bool isShift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

                if (isCtrl && isShift && key == Key.Q)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (Window window in Application.Current.Windows)
                        {
                            window.Close(); // Đóng toàn bộ cửa sổ
                        }

                        Application.Current.Shutdown(); // Thoát ứng dụng
                        Environment.Exit(0); // Đảm bảo thoát hẳn process nếu Shutdown bị chặn
                    });
                }

                // Chặn các phím hệ thống khác (Alt+Tab, Win, Ctrl+Alt+Del, ...)
                if (IsSystemKeyCombination(vkCode))
                {
                    return new IntPtr(1); // Chặn phím
                }
            }

            return WinApiHelper.CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            return WinApiHelper.CallNextHookEx(_mouseHookId, nCode, wParam, lParam);
        }

        private bool IsSystemKeyCombination(int vkCode)
        {
            bool isCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            bool isAlt = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
            bool isShift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            bool isWin = Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin);

            return
                (isCtrl && isAlt && vkCode == 0x2E) || // Ctrl+Alt+Del
                (isAlt && vkCode == 0x09) ||          // Alt+Tab
                (isCtrl && vkCode == 0x1B) ||         // Ctrl+Esc
                (isAlt && vkCode == 0x73) ||          // Alt+F4
                (isWin) ||                            // Windows key
                (isCtrl && isShift && vkCode == 0x1B); // Ctrl+Shift+Esc
        }

        public void Dispose()
        {
            WinApiHelper.UnhookWindowsHookEx(_keyboardHookId);
            WinApiHelper.UnhookWindowsHookEx(_mouseHookId);
        }
    }
}
