using LockScreenApp.Utilities;
using System;
using System.Runtime.InteropServices;
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
                if (IsSystemKeyCombination(vkCode, wParam))
                {
                    return new IntPtr(1);
                }
            }
            return WinApiHelper.CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // Add mouse event filtering if needed
            return WinApiHelper.CallNextHookEx(_mouseHookId, nCode, wParam, lParam);
        }

        private bool IsSystemKeyCombination(int vkCode, IntPtr wParam)
        {
            bool isCtrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            bool isAlt = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
            bool isWin = Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin);

            return (isCtrl && isAlt && vkCode == 0x2E) ||
                   (isAlt && vkCode == 0x09) ||
                   (isCtrl && vkCode == 0x1B) ||
                   (isAlt && vkCode == 0x73) ||
                   (isCtrl && Keyboard.IsKeyDown(Key.LeftShift) && vkCode == 0x1B) ||
                   isWin;
        }

        public void Dispose()
        {
            WinApiHelper.UnhookWindowsHookEx(_keyboardHookId);
            WinApiHelper.UnhookWindowsHookEx(_mouseHookId);
        }
    }
}
