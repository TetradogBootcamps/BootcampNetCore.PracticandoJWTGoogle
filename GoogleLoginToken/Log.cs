using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Gabriel.Cat.S.Blazor
{
    public static class Log
    {
        public static string FileName = $"log-{new DirectoryInfo(Environment.CurrentDirectory).Name}.txt";
        public static string FilePath => System.IO.Path.Combine(Environment.CurrentDirectory, FileName);
        public static bool OnlyOnDebug { get; set; } = false;
        static Semaphore Semaphore = new Semaphore(1, 1);
        public static void WriteLines(params string[] lines)
        {
            if (!OnlyOnDebug || System.Diagnostics.Debugger.IsAttached)
            {
                try
                {
                    Semaphore.WaitOne();
                    System.IO.File.AppendAllLines(FilePath, lines.Select(l => $"{DateTime.Now} ~ {l}"));
                }
                catch { }
                finally
                {
                    Semaphore.Release();
                }
            }

        }
        public static void Clear()
        {
            if (!OnlyOnDebug || System.Diagnostics.Debugger.IsAttached)
            {
                try
                {
                    Semaphore.WaitOne();
                    if (System.IO.File.Exists(FilePath))
                        System.IO.File.Delete(FilePath);
                }
                catch { }
                finally
                {
                    Semaphore.Release();
                }
            }
        }
    }
}
