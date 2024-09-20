namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger.Loggers
{
	using System;
	using System.IO;
	using System.Text;
	using System.Threading;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Models;

	public class PerformanceLogger : IPerformanceLogger
	{
		private const string DirectoryPath = @"C:\Skyline_Data\PerformanceLogger";
		private static readonly object _fileLock = new object();

		public PerformanceLogger(string fileName, string filePath = DirectoryPath)
		{
			if (String.IsNullOrEmpty(fileName)) throw new ArgumentException(nameof(fileName));
			if (String.IsNullOrEmpty(filePath)) throw new ArgumentException(nameof(filePath));

			FileName = fileName;
			FilePath = filePath;
		}

		public string FileName { get; set; }

		public string FilePath { get; set; }

		public bool IncludeDate { get; set; } = false;

		public bool AllowOverwrite { get; set; } = false;

		public bool LogPerThread { get; set; } = false;

		public void Report(PerformanceData data)
		{
			Directory.CreateDirectory(FilePath);

			string fileName = BuildFileName();
			string fullPath = Path.Combine(DirectoryPath, fileName);

			lock (_fileLock)
			{
				using (var fileStream = AllowOverwrite ? File.CreateText(fullPath) : File.AppendText(fullPath))
				{
					var jsonSerializer = new JsonSerializer
					{
						NullValueHandling = NullValueHandling.Ignore,
						ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
						DateFormatHandling = DateFormatHandling.IsoDateFormat,
					};

					jsonSerializer.Serialize(fileStream, data);
				}
			}
		}

		private string BuildFileName()
		{
			var sb = new StringBuilder();

			if (IncludeDate) sb.Append($"{DateTime.UtcNow:yyyy-MM-dd hh-mm-ss.fff}_");
			if (LogPerThread) sb.Append($"Thread_{Thread.CurrentThread.ManagedThreadId}_");
			sb.Append($"{FileName}.json");

			return sb.ToString();
		}
	}
}