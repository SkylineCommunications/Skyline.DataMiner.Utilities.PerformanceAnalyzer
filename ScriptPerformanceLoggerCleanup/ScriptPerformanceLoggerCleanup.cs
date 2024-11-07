namespace Skyline.DataMiner.Utils.ScriptPerformanceLoggerCleanup
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Skyline.DataMiner.Automation;

    public class Script
    {
        private string _performanceMetricsLocation;
        private DateTime _maxDaysSinceLastModified;
        private HashSet<string> _fileNamesToDelete;
        private bool _hasFailures;
        private IEngine _engine;

        public void Run(IEngine engine)
        {
            try
            {
                _engine = engine;
                RunSafe();
                if (_hasFailures) engine.ExitFail("Failed to delete some files. Check SLAutomation logging.");
            }
            catch (Exception ex)
            {
                engine.ExitFail("Something went wrong: " + ex.Message);
            }
        }

        private void RunSafe()
        {
            Initialize();

            if (!Directory.Exists(_performanceMetricsLocation))
            {
                throw new DirectoryNotFoundException("The directory does not exist.");
            }

            DetermineFilesToDelete();
            DeleteFiles();
        }

        private void Initialize()
        {
            _maxDaysSinceLastModified = GetOldestPerformanceDate();
            _performanceMetricsLocation = GetFolderPath();
            _fileNamesToDelete = new HashSet<string>();
        }

        private void DetermineFilesToDelete()
        {
            string[] files = Directory.GetFiles(_performanceMetricsLocation);

            foreach (string file in files)
            {
                if (File.GetLastWriteTime(file) < _maxDaysSinceLastModified)
                {
                    _fileNamesToDelete.Add(file);
                }
            }
        }

        private void DeleteFiles()
        {
            foreach (string fileName in _fileNamesToDelete)
            {
                try
                {
                    File.Delete(fileName);
                }
                catch (Exception ex)
                {
                    _hasFailures = true;
                    _engine.Log($"Failed to delete file: {fileName} - {ex.Message}");
                }
            }
        }

        private DateTime GetOldestPerformanceDate()
        {
            var inputOfDays = _engine.GetScriptParam("Max Days Since Last Modified")?.Value;
            if (string.IsNullOrEmpty(inputOfDays) || !int.TryParse(inputOfDays, out int days))
            {
                throw new ArgumentException("Invalid or missing value for Days of oldest performance info. It must be a valid integer.");
            }

            return DateTime.Now.AddDays(-days);
        }

        private string GetFolderPath()
        {
            var inputOfFolderPath = Convert.ToString(_engine.GetScriptParam("Performance Metrics Location")?.Value);
            if (string.IsNullOrEmpty(inputOfFolderPath))
            {
                throw new ArgumentException("Missing value for Folder path to performance info.");
            }

            return inputOfFolderPath.Trim();
        }
    }
}