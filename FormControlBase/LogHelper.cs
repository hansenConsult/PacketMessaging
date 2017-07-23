using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using MetroLog;

namespace FormControlBaseClass
{
    public class LogHelper
    {
        private static ILogger log = LogManagerFactory.DefaultLogManager.GetLogger<LogHelper>();

        // Log
        public static void Log(LogLevel logLevel, string message, [CallerMemberName] string memberName = "",
                [CallerFilePath] string sourceFilePath = "",
                [CallerLineNumber] int sourceLineNumber = 0)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    log.Trace($"{message}, Line = {sourceLineNumber}");
                    break;
                case LogLevel.Debug:
                    log.Debug($"{message}, Line = {sourceLineNumber}");
                    break;
                case LogLevel.Info:
                    log.Info($"{message}, Line = {sourceLineNumber}");
                    break;
                case LogLevel.Warn:
                    log.Warn($"{message}, Line = {sourceLineNumber}");
                    break;
                case LogLevel.Error:
                    log.Error($"{message}, Line = {sourceLineNumber}");
                    break;
                case LogLevel.Fatal:
                    log.Fatal($"{message}, Line = {sourceLineNumber}");
                    break;
            }
        }

    }
}
