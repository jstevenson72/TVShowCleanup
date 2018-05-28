using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using TVShowCleanup.Utilities;

namespace TVShowCleanup
{
    public static class CleanupLog
    {
        private static int _eventCounter;

        static CleanupLog()
        {
            LogHelper.Log.Setup(CleanupHelper.DataPath, "CleanupLog.txt");

            LogHelper.Log.WriteLog("----------------------------");
            LogHelper.Log.WriteLog($" Exe Path: {Assembly.GetEntryAssembly().Location} ");
            LogHelper.Log.WriteLog($" Config Path: {CleanupHelper.DataPath}");
            LogHelper.Log.WriteLog("----------------------------");
        }

        public static void HandleException(object sender, Exception exception)
        {
            try
            {
                // Create Error Message
                var message = $"An Application Error has occurred.\nEXCEPTION:\n{exception}";

                // Log Message to EventLog
                WriteLine(message);
            }
            catch
            {
                // ignored
            }
        }

        public static void WriteLine(string eventLogMessage)
        {
            try
            {
                _eventCounter++;

                var message = eventLogMessage.Replace("\t", @"\t");
                var output  = $"({_eventCounter}): {message}";
                
                LogHelper.Log.WriteLog(@output);
            }
            catch (Exception)
            {
            }
        }

        public static void WriteMethod(string message = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var file = Path.GetFileName(sourceFilePath);
            WriteLine($"[{file}][{memberName}({message})][Line {sourceLineNumber}]");
        }
    }
}