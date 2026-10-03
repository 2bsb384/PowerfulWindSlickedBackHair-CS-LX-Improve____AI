using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;

namespace PowerfulWindSlickedBackHair.Tools
{
    /// <summary>
    /// 从嵌入资源加载素材。Assets 目录下文件以 "PowerfulWindSlickedBackHair.Assets.文件名" 嵌入。
    /// </summary>
    public static class AssetHelper
    {
        private const string AssetPrefix = "PowerfulWindSlickedBackHair.Assets.";
        private const string RootPrefix = "PowerfulWindSlickedBackHair.";

        private static readonly Assembly asm = Assembly.GetExecutingAssembly();

        private static Stream Open(string resourceName)
        {
            Stream s = asm.GetManifestResourceStream(resourceName);
            if (s == null)
            {
                throw new FileNotFoundException("嵌入资源不存在: " + resourceName);
            }
            return s;
        }

        /// <summary>Assets 目录下的资源流</summary>
        public static Stream OpenAsset(string fileName)
        {
            return Open(AssetPrefix + fileName);
        }

        /// <summary>Assets 目录下的图片</summary>
        public static Bitmap GetAssetBitmap(string fileName)
        {
            using (Stream s = OpenAsset(fileName))
            {
                return new Bitmap(s);
            }
        }

        /// <summary>Assets 目录下的图标</summary>
        public static Icon GetAssetIcon(string fileName)
        {
            using (Stream s = OpenAsset(fileName))
            {
                return new Icon(s);
            }
        }

        /// <summary>项目根目录（非 Assets）嵌入的资源流</summary>
        public static Stream OpenRoot(string fileName)
        {
            return Open(RootPrefix + fileName);
        }

        /// <summary>
        /// 提取嵌入资源到临时目录并返回文件路径（已存在且非空则复用）。
        /// </summary>
        public static string ExtractAssetToTemp(string fileName)
        {
            return ExtractToTemp(AssetPrefix + fileName, fileName);
        }

        /// <summary>
        /// 提取项目根目录嵌入资源到指定目录并返回文件路径（已存在且非空则复用）。
        /// </summary>
        public static string ExtractRootTo(string fileName, string directory)
        {
            string file = Path.Combine(directory, fileName);
            if (!File.Exists(file) || new FileInfo(file).Length == 0)
            {
                using (Stream s = OpenRoot(fileName))
                using (FileStream f = File.Create(file))
                {
                    s.CopyTo(f);
                }
            }
            return file;
        }

        /// <summary>
        /// 提取嵌入资源为临时文件（供需要真实文件路径的 API 使用，如 NAudio、字体、壁纸）。
        /// </summary>
        public static string ExtractToTemp(string resourceName, string fileName)
        {
            string dir = Path.Combine(Path.GetTempPath(), "PWBSBH");
            Directory.CreateDirectory(dir);
            string file = Path.Combine(dir, fileName);
            if (!File.Exists(file) || new FileInfo(file).Length == 0)
            {
                using (Stream s = Open(resourceName))
                using (FileStream f = File.Create(file))
                {
                    s.CopyTo(f);
                }
            }
            return file;
        }

        private static int cleanupScheduled;

        private static string PSQuote(string s)
        {
            return "'" + s.Replace("'", "''") + "'";
        }

        /// <summary>
        /// 安排进程退出后清理运行时释放的文件：%TEMP%\PWBSBH 临时目录与 exe 旁的 SoundTouch 原生库，
        /// 只保留原 exe。原生 DLL 运行期间被锁定，字体/音频文件被本进程持有，因此由隐藏的
        /// 外部进程等待本进程结束后再删除。
        /// </summary>
        public static void ScheduleCleanupOnExit()
        {
            if (System.Threading.Interlocked.Exchange(ref cleanupScheduled, 1) == 1) return;
            try
            {
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string tempDir = Path.Combine(Path.GetTempPath(), "PWBSBH");

                string script =
                    "$ErrorActionPreference='SilentlyContinue'\n" +
                    "Wait-Process -Id " + Process.GetCurrentProcess().Id + "\n" +
                    "Start-Sleep -Milliseconds 1200\n" +
                    "Remove-Item -LiteralPath " + PSQuote(tempDir) + " -Recurse -Force\n" +
                    "Remove-Item -LiteralPath " + PSQuote(Path.Combine(exeDir, "SoundTouch.dll")) + "," +
                    PSQuote(Path.Combine(exeDir, "SoundTouch_x64.dll")) + " -Force\n";

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-NoProfile -WindowStyle Hidden -EncodedCommand " +
                        Convert.ToBase64String(Encoding.Unicode.GetBytes(script)),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = exeDir
                };
                Process.Start(psi);
            }
            catch
            {
                // 清理安排失败不影响主流程
            }
        }
    }
}