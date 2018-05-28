using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace TVShowCleanup.Utilities
{
    public class Log
    {
        private string _logFileDirectory = string.Empty;
        private string _logFileName = string.Empty;
        private string _logFilePath = string.Empty;
        private readonly object _lock = new object();
        
        public static DateTime GetDateTimeForEvent(string eventMessage)
        {
            var dateTime = new DateTime();

            var dateTimeString = eventMessage.Split(']')
                                             .FirstOrDefault();

            if (dateTimeString != null)
            {
                dateTimeString = dateTimeString.TrimStart('[');

                DateTime.TryParse(dateTimeString, out dateTime);
            }

            return dateTime;
        }

        public string ReadLog()
        {
            if (string.IsNullOrWhiteSpace(_logFilePath))
            {
                return null;
            }

            var logEntries = string.Empty;

            try
            {
                using (var fileStream = new FileStream(_logFilePath, FileMode.Open, FileAccess.Read))
                {
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        logEntries = streamReader.ReadToEnd();
                    }
                }
            }
            catch (Exception)
            {
                // ignore
            }

            return logEntries;
        }

        public List<string> ReadLogItems(bool includeSourceFileName = false)
        {
            if (string.IsNullOrWhiteSpace(_logFilePath))
            {
                return null;
            }

            var logEntries = new List<string>();

            try
            {
                using (var fileStream = new FileStream(_logFilePath, FileMode.Open, FileAccess.Read))
                {
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        var eventMessage = new StringBuilder();

                        while (!streamReader.EndOfStream)
                        {
                            var readLine = streamReader.ReadLine();
                            if (readLine != null
                                && readLine.StartsWith("["))
                            {
                                logEntries.Add(eventMessage + (includeSourceFileName
                                    ? ", Source: " + _logFileName
                                    : string.Empty));
                                eventMessage = new StringBuilder(readLine);
                            }
                            else
                            {
                                eventMessage.Append(readLine);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignore
            }

            return logEntries;
        }

        public void Setup(string logFileParentDirectory, string logFileName)
        {
            _logFileDirectory = logFileParentDirectory;

            if (!Directory.Exists(_logFileDirectory))
            {
                Directory.CreateDirectory(_logFileDirectory);
            }

            _logFileName = logFileName;
            _logFilePath = Path.Combine(_logFileDirectory, _logFileName);
        }

        public void WriteLog(string message, params object[] parameters)
        {
            if (string.IsNullOrWhiteSpace(_logFilePath))
            {
                return;
            }

            try
            {
                lock (_lock)
                {
                    // 10MB Limit.
                    CheckLogSizeForRoll(10485760);

                    if (parameters != null
                        && parameters.Any())
                    {
                        message = string.Format(message, parameters);
                    }

                    message = $"[{DateTime.Now}] {@message}";

                    Debug.WriteLine(@message);

                    using (var fileStream = new FileStream(_logFilePath, FileMode.Append, FileAccess.Write))
                    {
                        using (var streamWriter = new StreamWriter(fileStream))
                        {
                            streamWriter.WriteLine(message);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }

        public void WriteLogBypassRollCheck(string message, params object[] parameters)
        {
            if (string.IsNullOrWhiteSpace(_logFilePath))
            {
                return;
            }

            if (parameters != null)
            {
                message = string.Format(message, parameters);
            }

            try
            {
                lock (_lock)
                {
                    Debug.WriteLine(message);

                    message = string.Format("[{0}] {1}", DateTime.Now, message);

                    using (var fileStream = new FileStream(_logFilePath, FileMode.Append, FileAccess.Write))
                    {
                        using (var streamWriter = new StreamWriter(fileStream))
                        {
                            streamWriter.WriteLine(message);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private void CheckLogSizeForRoll(int maxLogSize)
        {
            if (string.IsNullOrWhiteSpace(_logFilePath))
            {
                return;
            }

            // Check if file size is greater than 2MB, then roll.
            var fileInfo = new FileInfo(_logFilePath);
            if (fileInfo.Exists)
            {
                if (fileInfo.Length > maxLogSize)
                {
                    WriteLogBypassRollCheck("CheckLogSizeForRoll: Rolling existing log file; Current Size - {0}; Maximum Size - {1}.", fileInfo.Length, maxLogSize);
                    RollLog();
                }
            }
        }

        private static string CleanTimeString(string input)
        {
            return input.Replace(":", string.Empty)
                        .Replace("/", string.Empty)
                        .Replace("-", string.Empty);
        }

        private void DeleteOldLogs(int maxLogAge)
        {
            if (string.IsNullOrWhiteSpace(_logFilePath))
            {
                return;
            }

            if (!Directory.Exists(_logFilePath))
            {
                return;
            }

            try
            {

                // ReSharper disable once AssignNullToNotNullAttribute
                var files = Directory.GetFiles(Path.GetDirectoryName(_logFilePath));

                files.ToList()
                     .ForEach(file =>
                              {
                                  if (file.Contains("-" + _logFileName))
                                  {
                                      var fileInfo = new FileInfo(file);
                                      if (fileInfo.Exists)
                                      {
                                          WriteLog("DeleteOldLogs: Found archived log file - {0}.", file);

                                          if (fileInfo.CreationTime.Date < DateTime.Now.Date.AddDays(maxLogAge * -1))
                                          {
                                              WriteLog("DeleteOldLogs: Age of log file exceeds {0}, deleting.", maxLogAge);
                                              fileInfo.Delete();
                                          }
                                      }
                                  }
                              });
            }
            catch (Exception exception)
            {
                WriteLog("DeleteOldLogs: Exception - {0}", exception);
            }
        }

        private void RollLog()
        {
            if (string.IsNullOrWhiteSpace(_logFilePath))
            {
                return;
            }

            var moveToLocation = Path.Combine(_logFileDirectory, string.Format("{0}{1}-{2}", CleanTimeString(DateTime.Now.ToShortDateString()), CleanTimeString(DateTime.Now.ToShortTimeString()), _logFileName));
            if (File.Exists(_logFilePath))
            {
                WriteLogBypassRollCheck("RollLog: Rolling existing log file - {0}.", moveToLocation);
                File.Move(_logFilePath, moveToLocation);
            }

            // Clean up any logs older than 5 days.
            DeleteOldLogs(5);
        }
    }
}