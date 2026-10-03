using PowerfulWindSlickedBackHair.Tools;
using System;
using System.Threading;
using System.Windows.Forms;

namespace PowerfulWindSlickedBackHair
{
    // Token: 0x02000003 RID: 3
    internal static class Program
    {
        // Token: 0x0600000A RID: 10 RVA: 0x0000323C File Offset: 0x0000143C
        [STAThread]
        private static void Main()
        {
            // 单文件运行：释放原生 SoundTouch 依赖到 exe 目录，并注册嵌入的 NAudio 程序集加载
            AssetHelper.ExtractRootTo("SoundTouch.dll", AppDomain.CurrentDomain.BaseDirectory);
            AssetHelper.ExtractRootTo("SoundTouch_x64.dll", AppDomain.CurrentDomain.BaseDirectory);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.ThreadException += Application_ThreadException;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string name = new System.Reflection.AssemblyName(args.Name).Name;
            if (name == "NAudio")
            {
                using (var s = AssetHelper.OpenRoot("NAudio.dll"))
                {
                    using (var ms = new System.IO.MemoryStream())
                    {
                        s.CopyTo(ms);
                        return System.Reflection.Assembly.Load(ms.ToArray());
                    }
                }
            }
            return null;
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            var ex = e.Exception;
            Logger.WriteLine(ex);
            // 锁定期间不能弹窗，直接恢复系统状态
            SystemLockHelper.UnlockAll();
            Win32APIHelper.ChangeWallpaper(0);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            Logger.WriteLine(ex);
            SystemLockHelper.UnlockAll();
            if (e.IsTerminating)
            {
                Win32APIHelper.ChangeWallpaper(0);
                // 等壁纸恢复落地后再退出，进程退出后由外部进程清理释放的文件
                Thread.Sleep(1500);
                AssetHelper.ScheduleCleanupOnExit();
            }
        }

    }
}
