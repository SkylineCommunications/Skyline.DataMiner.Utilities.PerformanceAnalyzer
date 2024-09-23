namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger.Loggers
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Models;

	public class PerformanceLogger : IPerformanceLogger
	{
		private const string DirectoryPath = @"C:\Skyline_Data\PerformanceLogger";
		private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore,
			ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
			DateFormatHandling = DateFormatHandling.IsoDateFormat,
		};

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

		// ESUDBG this doesn't work
		public bool LogPerThread { get; set; } = false;

		public void Report(List<PerformanceData> data)
		{
			Directory.CreateDirectory(FilePath);

			string fileName = BuildFileName();
			string fullPath = Path.Combine(DirectoryPath, fileName);

			lock (_fileLock)
			{
				using (var fileStream = new FileStream(fullPath, FileMode.OpenOrCreate))
				{
					fileStream.Position = GetStartPosition(fileStream);

					using (var writer = new StreamWriter(fileStream))
					{
						string prefix = fileStream.Position == 0 ? "[" : ",";

						writer.WriteLine(prefix + JsonConvert.SerializeObject(data.Where(d => d != null), _jsonSerializerSettings).TrimStart('['));
					}
				}
			}
		}

		private long GetStartPosition(FileStream fileStream)
		{
			char searchChar = '}';
			bool charFound = false;

			long position = fileStream.Length - 1;

			while (position >= 0 && !charFound)
			{
				fileStream.Seek(position, SeekOrigin.Begin);

				int currentByte = fileStream.ReadByte();

				if (currentByte == -1)
					return 0;

				if ((char)currentByte == searchChar)
					return position + 1;

				position--;
			}

			return 0;
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