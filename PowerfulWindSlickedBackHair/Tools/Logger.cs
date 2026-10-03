using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PowerfulWindSlickedBackHair.Tools
{
    public static class Logger
    {
        private static readonly object _lock = new object();

        private static readonly string _logFilePath = CreateLogPath();

        private static string CreateLogPath()
        {
            // 日志写进临时释放目录（%TEMP%\PWBSBH），随退出清理一并删除，不留在 exe 旁
            string dir = Path.Combine(Path.GetTempPath(), "PWBSBH");
            try
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
            catch (Exception)
            {
            }
            return Path.Combine(dir, "log.txt");
        }

        public static void WriteLine(string msg)
        {
            try
            {
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - INFO: {msg}";

                lock (_lock)
                {
                    using (StreamWriter writer = new StreamWriter(_logFilePath, true, Encoding.UTF8))
                    {
                        writer.WriteLine(logMessage);
                        writer.Flush();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public static void WriteLine(Exception ex)
        {
            try
            {
                string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR: {ex.Message}\r\n{ex}";
                if (ex.InnerException != null)
                {
                    logMessage += $"\r\nInner Exception: {ex.InnerException}";
                }

                lock (_lock)
                {
                    using (StreamWriter writer = new StreamWriter(_logFilePath, true, Encoding.UTF8))
                    {
                        writer.WriteLine(logMessage);
                        writer.WriteLine(new string('-', 80));
                        writer.Flush();
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
