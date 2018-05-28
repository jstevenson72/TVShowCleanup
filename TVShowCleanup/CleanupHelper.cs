using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using TVShowCleanup.Utilities;

namespace TVShowCleanup
{
    public class CleanupHelper
    {
        public static Configuration ConfigurationSettings;
        public static string DataPath { get; internal set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"TVShowCleanup\");

        private static BlockingQueue<string> _directoryToCleanupQueue;
        private static string _configurationFilePath;
        private static bool _isCleaning = false;
        private static System.Timers.Timer _cleanupTimer = new System.Timers.Timer();
        private static List<FileSystemWatcher> _fileWatchers = new List<FileSystemWatcher>();
        private static bool _continueRunning;
        private static BigInteger _numberOfFilesProcessed = 0;
        private static BigInteger _numberOfFilesDeleted = 0;

        public static void StartCleanup()
        {
            CleanupLog.WriteMethod();

            _directoryToCleanupQueue = new BlockingQueue<string>(new Queue<string>());

            _continueRunning = true;

            // Initialize Configuration
            ReadConfigFile();

            // Connect to Remote Shares
            ConnectToRemoteShares();

            // Setup Global Cleanup Timer
            if (ConfigurationSettings.EnableGlobalTimerCleanup)
            {
                _cleanupTimer.Interval = TimeHelper.ConvertMinutesToMilliseconds(ConfigurationSettings.HowOftenInMinutesToCleanup);
                _cleanupTimer.Elapsed += (s, e) => Cleanup();
                _cleanupTimer.Start();
            }

            // Setup Individual Cleanup Directory Watchers
            if (ConfigurationSettings.EnableFileWatcherCleanup)
            {
                ConfigurationSettings.DirectoriesToCleanup.ForEach(d => AddDirectoryToCleanup(d));
            }

            // Perform Initial Cleanup
            if (ConfigurationSettings.EnableGlobalTimerCleanup || ConfigurationSettings.EnableFileWatcherCleanup)
            {
                _continueRunning = true;

                Cleanup();

                CleanupLog.WriteLine($"Initial Cleanup Totals:  Found {_numberOfFilesProcessed} files, Deleted {_numberOfFilesDeleted} files.");
            }
            else
            {
                CleanupLog.WriteLine("No Cleanup is enabled. Exiting");

                StopCleanup();

                return;
            }

            // Monitor Directories for Change, Checking Delete Queue Until Aborted
            while (_continueRunning)
            {
                Thread.Sleep(3000); // Wait 3 Seconds between Checks

                if (_isCleaning) return;

                var path = _directoryToCleanupQueue.Dequeue();
                if (path != null)
                {
                    CleanupFolder(path);
                }

                CleanupLog.WriteLine($"Cleanup Totals:  Found {_numberOfFilesProcessed} files, Deleted {_numberOfFilesDeleted} files.");
            }
        }

        private static void Cleanup()
        {
            CleanupLog.WriteMethod();

            CleanupLog.WriteLine($"Directories to Clean: {ConfigurationSettings.DirectoriesToCleanup.Count()}");

            // Queue All Directories for Cleanup
            if (ConfigurationSettings.DirectoriesToCleanup.Any())
            {
                ConfigurationSettings.DirectoriesToCleanup.ForEach(d => AddFolderToQueue(d));
            }

            CleanupLog.WriteLine(".");

            // Process Queue One Time
            while (_directoryToCleanupQueue.HasItems())
            {
                CleanupLog.WriteLine("..");

                var path = _directoryToCleanupQueue.Dequeue();
                if (path != null)
                {
                    CleanupFolder(path);
                }
            }

            CleanupLog.WriteLine("...");

        }

        private static void ConnectToRemoteShares()
        {
            CleanupLog.WriteMethod();
            List<string> remoteShares = new List<string>();
            ConfigurationSettings.DirectoriesToCleanup.ForEach(path =>
            {
                if (path.StartsWith(@"\\"))
                {
                    var rootPath = Path.GetPathRoot(path);
                    remoteShares.Add(rootPath.Trim().ToUpper());
                }
            });

            CleanupLog.WriteLine($"{remoteShares.Distinct().Count()} Remote Shares Needed.");

            remoteShares.Distinct().ToList().ForEach(path =>
            {
                CleanupLog.WriteLine($"Connecting to Remote Share: {path}, with: Username: {ConfigurationSettings.Username}, Password: {ConfigurationSettings.Password}");
                var connection = new PersistentConnection(path, new System.Net.NetworkCredential(ConfigurationSettings.Username, ConfigurationSettings.Password));
            });
        }

        private static void AddFolderToQueue(string path)
        {
            if (_directoryToCleanupQueue.HasItem(path))
            {
                //CleanupLog.WriteLine($"Path already in Queue: {path}");
                return;
            }

            //CleanupLog.WriteMethod(path);            
            _directoryToCleanupQueue.Enqueue(path);
        }

        private static void CleanupFolder(string path)
        {
            _isCleaning = true;

            CleanupLog.WriteMethod(path);

            if (Directory.Exists(path))
            {
                DoCleanup(path);
            }
            else
            {
                CleanupLog.WriteLine($"Directory Does Not Exist: {path}");
            }

            _isCleaning = false;
        }

        private static void DoCleanup(string path)
        {
            // Find Files to Delete
            var allfiles = Directory.GetFiles(path, "*", SearchOption.AllDirectories)
            .Select(f => new
            {
                ModifiedDate = File.GetLastWriteTime(f),
                Path = f
            })
            .OrderByDescending(f => f.ModifiedDate)
            .ToList();

            _numberOfFilesProcessed += allfiles.Count();

            CleanupLog.WriteLine($"Found {allfiles.Count} files.");

            var filesToKeep = allfiles.Take(ConfigurationSettings.MinimumNumberOfFilesToKeep);

            var filesEligibleForDeletion = allfiles.Except(filesToKeep);

            var filesToDelete = filesEligibleForDeletion
                .Where(f => f.ModifiedDate < DateTime.Now.AddDays(-ConfigurationSettings.KeepFilesNewerThanThisNumberOfDays))
                .ToList();

            CleanupLog.WriteLine($"Found {filesToDelete.Count} files eligible for deletion.");

            _numberOfFilesDeleted += filesToDelete.Count();

            // Delete Files
            Parallel.ForEach(filesToDelete, currentFile =>
            {
                CleanupLog.WriteLine($"Deleting {currentFile.Path}.");
                File.SetAttributes(currentFile.Path, FileAttributes.Normal);
                File.Delete(currentFile.Path);
            });

            // Find and Delete Empty Directories
            DirectoryHelper.DeleteEmptyDirectories(path);

            CleanupLog.WriteLine($"Cleanup Complete for {path}.");
        }

        private static void ReadConfigFile()
        {
            CleanupLog.WriteMethod();

            _configurationFilePath = Path.Combine(DataPath, "Configuration.txt");

            CleanupLog.WriteLine($"Configuration Settings: {_configurationFilePath}");

            var serializeHelper = new SerializeHelper<Configuration>();

            // Configure Settings
            if (File.Exists(_configurationFilePath))
            {
                // Read Configuration Settings
                ConfigurationSettings = serializeHelper.ReadJSONSettings(_configurationFilePath);
            }
            else
            {
                // Default to new Configuration Settings
                ConfigurationSettings = new Configuration()
                {
                    DirectoriesToCleanup = new List<string> { @"c:\temp", @"c:\temp2" },
                    HowOftenInMinutesToCleanup = 5,
                    EnableGlobalTimerCleanup = false,
                    EnableFileWatcherCleanup = false
                };

                serializeHelper.WriteJSONSettings(_configurationFilePath, ConfigurationSettings);
            }
        }

        private static void AddDirectoryToCleanup(string path)
        {
            CleanupLog.WriteMethod(path);

            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = path;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = "*.*";
            watcher.Changed += (s, e) => AddFolderToQueue(path);
            watcher.EnableRaisingEvents = true;

            _fileWatchers.Add(watcher);
        }

        public static void StopCleanup()
        {
            CleanupLog.WriteMethod();

            _cleanupTimer?.Stop();

            if (_fileWatchers.Any())
            {
                _fileWatchers.ForEach(_ => _.EnableRaisingEvents = false);
            }

            _continueRunning = false;
        }
    }
}
