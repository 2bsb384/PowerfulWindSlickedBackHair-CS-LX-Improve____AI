using System;
using System.Runtime.InteropServices;

namespace PowerfulWindSlickedBackHair.Tools
{
    /// <summary>
    /// 锁定桌面：隐藏桌面图标、隐藏任务栏、禁用鼠标键盘输入；退出时恢复。
    /// 说明：只隐藏桌面图标列表（SysListView32），不隐藏 SHELLDLL_DefView/Progman，
    /// 以保证壁纸（由 WorkerW 层绘制）不受影响。
    /// </summary>
    public static class SystemLockHelper
    {
        [DllImport("user32.dll")]
        private static extern bool BlockInput(bool fBlockIt);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private const int SW_RESTORE = 9;

        private static IntPtr desktopIconList = IntPtr.Zero;
        private static IntPtr taskbar = IntPtr.Zero;
        private static bool isLocked;

        /// <summary>
        /// 查找桌面图标层窗口 SHELLDLL_DefView（Progman 或它的 WorkerW 子窗口）
        /// </summary>
        private static IntPtr FindDesktopDefView()
        {
            IntPtr progman = FindWindow("Progman", null);
            IntPtr defView = IntPtr.Zero;
            if (progman != IntPtr.Zero)
            {
                defView = FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);
            }
            if (defView != IntPtr.Zero)
            {
                return defView;
            }
            // 部分系统上 DefView 挂在 WorkerW 下
            IntPtr workerW = IntPtr.Zero;
            while ((workerW = FindWindowEx(IntPtr.Zero, workerW, "WorkerW", null)) != IntPtr.Zero)
            {
                defView = FindWindowEx(workerW, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (defView != IntPtr.Zero)
                {
                    return defView;
                }
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// 隐藏桌面图标和任务栏，并禁用鼠标键盘输入（壁纸保持显示）。
        /// </summary>
        public static void LockAll()
        {
            if (isLocked) return;
            isLocked = true;

            taskbar = FindWindow("Shell_TrayWnd", null);
            if (taskbar != IntPtr.Zero)
            {
                ShowWindow(taskbar, SW_HIDE);
            }

            IntPtr defView = FindDesktopDefView();
            if (defView != IntPtr.Zero)
            {
                // 只隐藏图标列表，保留 DefView/Progman 可见，壁纸不会被去掉
                desktopIconList = FindWindowEx(defView, IntPtr.Zero, "SysListView32", null);
                if (desktopIconList == IntPtr.Zero)
                {
                    // 极少数系统没有 SysListView32，退回隐藏整个 DefView
                    desktopIconList = defView;
                }
                ShowWindow(desktopIconList, SW_HIDE);
            }

            BlockInput(true);
        }

        /// <summary>
        /// 恢复桌面图标、任务栏和输入。
        /// </summary>
        public static void UnlockAll()
        {
            BlockInput(false);
            if (desktopIconList != IntPtr.Zero)
            {
                ShowWindow(desktopIconList, SW_SHOW);
            }
            if (taskbar != IntPtr.Zero)
            {
                ShowWindow(taskbar, SW_RESTORE);
            }
            desktopIconList = IntPtr.Zero;
            taskbar = IntPtr.Zero;
            isLocked = false;
        }
    }
}