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
            } // REMARK: Why bother catching these specific exceptions?
            catch (DirectoryNotFoundException ex)
            {
                engine.ExitFail("Directory not found: " + ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                engine.ExitFail("Access denied: " + ex.Message);
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
            var inputOfDays = engine.GetScriptParam("Days of oldest performance info")?.Value;
            if (string.IsNullOrEmpty(inputOfDays) || !int.TryParse(inputOfDays, out int days))
            {
                throw new ArgumentException("Invalid or missing value for Days of oldest performance info. It must be a valid integer.");
            }

            return DateTime.Now.AddDays(-days);
        }

        private static string GetFolderPath(IEngine engine)
        {
            var inputOfFolderPath = Convert.ToString(engine.GetScriptParam("Folder path to performance info")?.Value);
            if (string.IsNullOrEmpty(inputOfFolderPath))
            {
                throw new ArgumentException("Missing value for Folder path to performance info.");
            }

            return inputOfFolderPath.Trim();
        }

        private static void TryDeleteFile(IEngine engine, string fileName)
        {
            // REMARK: Does it make sense to stop the cleanup as soon as any of the files cannot be removed?
            try
            {
                File.Delete(fileName);
            }
            catch (UnauthorizedAccessException ex)
            {
                // REMARK: calling engine.ExitFail causes an exception to be thrown, which you will then catch in the Run method and wrap the message
                engine.ExitFail($"Unauthorized Access | Failed to delete file: {fileName} - {ex.Message}");
            }
            catch (IOException ex)
            {
                engine.ExitFail($"IO Exception | Failed to delete file: {fileName} - {ex.Message}");
            }
            catch (Exception ex)
            {
                engine.ExitFail($"Exception | Failed to delete file: {fileName} - {ex.Message}");
            }
        }

        private void DeleteFiles(IEngine engine)
        {
            foreach (string fileName in fileNamesToDelete)
            {
                TryDeleteFile(engine, fileName);
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
            DeleteFiles(engine);
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