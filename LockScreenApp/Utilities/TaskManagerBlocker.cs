using Microsoft.Win32;
using System;
using System.Windows;

namespace LockScreenApp.Utilities
{
    public static class TaskManagerBlocker
    {
        public static void DisableTaskManager()
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);

                if (key != null)
                {
                    key.SetValue("DisableTaskMgr", 1, RegistryValueKind.DWord);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể khóa Task Manager: " + ex.Message);
            }
        }

        public static void EnableTaskManager()
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);

                if (key != null)
                {
                    key.DeleteValue("DisableTaskMgr", false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể mở lại Task Manager: " + ex.Message);
            }
        }
    }
}
