namespace Skyline.DataMiner.Utils.ScriptPerformanceLoggerCleanup
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Skyline.DataMiner.Automation;

    public class Script
    {
        private string folderPath;
        private DateTime oldestPerformanceInfoDateTime;
        private HashSet<string> fileNamesToDelete;

        public void Run(IEngine engine)
        {
            try
            {
                RunSafe(engine);
            }
            catch (Exception ex)
            {
                engine.ExitFail("Something went wrong: " + ex.Message);
            }
        }

        public void Initialize(IEngine engine)
        {
            oldestPerformanceInfoDateTime = GetOldestPerformanceDate(engine);
            folderPath = GetFolderPath(engine);
            fileNamesToDelete = new HashSet<string>();
        }

        private static DateTime GetOldestPerformanceDate(IEngine engine)
        {
            var inputOfDays = engine.GetScriptParam("Max Days Since Last Modified")?.Value;
            if (string.IsNullOrEmpty(inputOfDays) || !int.TryParse(inputOfDays, out int days))
            {
                throw new ArgumentException("Invalid or missing value for Days of oldest performance info. It must be a valid integer.");
            }

            return DateTime.Now.AddDays(-days);
        }

        private static string GetFolderPath(IEngine engine)
        {
            var inputOfFolderPath = Convert.ToString(engine.GetScriptParam("Performance Metrics Location")?.Value);
            if (string.IsNullOrEmpty(inputOfFolderPath))
            {
                throw new ArgumentException("Missing value for Folder path to performance info.");
            }

            return inputOfFolderPath.Trim();
        }

        private void DeleteFiles()
        {
            foreach (string fileName in fileNamesToDelete)
            {
                File.Delete(fileName);
            }
        }

        private void RunSafe(IEngine engine)
        {
            Initialize(engine);

            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException("The directory does not exist.");
            }

            DetermineFilesToDelete();
            DeleteFiles();
        }

        private void DetermineFilesToDelete()
        {
            string[] files = Directory.GetFiles(folderPath);

            foreach (string file in files)
            {
                if (File.GetLastWriteTime(file) < oldestPerformanceInfoDateTime)
                {
                    fileNamesToDelete.Add(file);
                }
            }
        }
    }
}