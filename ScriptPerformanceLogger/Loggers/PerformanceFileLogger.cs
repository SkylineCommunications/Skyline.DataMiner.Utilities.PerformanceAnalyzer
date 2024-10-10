namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger.Loggers
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Models;
	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Tools;

	/// <summary>
	/// <see cref="PerformanceFileLogger"/> is implementation of the <see cref="IPerformanceLogger"/> that logs to files.
	/// </summary>
	public class PerformanceFileLogger : IPerformanceLogger
	{
		private const string DirectoryPath = @"C:\Skyline_Data\PerformanceLogger";

		private static readonly object _fileLock = new object();
		private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore,
			ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
			DateFormatHandling = DateFormatHandling.IsoDateFormat,
		};

		private readonly Dictionary<string, string> _metadata = new Dictionary<string, string>();

		/// <summary>
		/// Initializes a new instance of the <see cref="PerformanceFileLogger"/> class.
		/// </summary>
		public PerformanceFileLogger()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PerformanceFileLogger"/> class.
		/// </summary>
		/// <param name="fileName">Name of the file to which to log.</param>
		/// <param name="filePath">Path of the <paramref name="fileName"/>.</param>
		/// <exception cref="ArgumentException">Throws if <paramref name="fileName"/> or <paramref name="filePath"/> are null or empty.</exception>
		public PerformanceFileLogger(string fileName, string filePath = DirectoryPath) : this(new LogFileInfo(fileName, filePath))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PerformanceFileLogger"/> class.
		/// </summary>
		/// <param name="logFileInfo">Array of files to which to log.</param>
		public PerformanceFileLogger(params LogFileInfo[] logFileInfo)
		{
			Array.ForEach(logFileInfo, x => LogFiles.Add(x));
		}

		/// <summary>
		/// Gets list of all log files.
		/// </summary>
		public List<LogFileInfo> LogFiles { get; private set; } = new List<LogFileInfo>();

		public Dictionary<string, string> Metadata => _metadata;

		/// <summary>
		/// Gets or sets a value indicating whether date should be included in files names.
		/// </summary>
		public bool IncludeDate { get; set; } = false;

		/// <summary>
		/// Logs specified data in to files.
		/// </summary>
		/// <param name="data">List of performance metrics to log.</param>
		public void Report(List<PerformanceData> data)
		{
			Retry.Execute(
				() => Store(data),
				TimeSpan.FromMilliseconds(100),
				tryCount: 10);
		}

		/// <summary>
		/// Adds metadata for the logs.
		/// </summary>
		/// <param name="key">Key of the metadata.</param>
		/// <param name="value">Value of the metadata.</param>
		/// <returns>Returns current instance of <see cref="PerformanceFileLogger"/>.</returns>
		public PerformanceFileLogger AddMetadata(string key, string value)
		{
			_metadata[key] = value;
			return this;
		}

		/// <summary>
		/// Adds metadata for the logs.
		/// </summary>
		/// <param name="metadata">Metadata to add or update to the <see cref="PerformanceFileLogger"/>.</param>
		/// <returns>Returns current instance of <see cref="PerformanceFileLogger"/>.</returns>
		public PerformanceFileLogger AddMetadata(IReadOnlyDictionary<string, string> metadata)
		{
			foreach (var data in metadata)
			{
				_metadata.[data.Key] = data.Value;
			}

			return this;
		}

		private static long GetStartPosition(FileStream fileStream)
		{
			char searchChar = '}';

			long position = fileStream.Length - 1;

			while (position >= 0)
			{
				fileStream.Seek(position, SeekOrigin.Begin);

				int currentByte = fileStream.ReadByte();

				if (currentByte == -1)
				{
					return 0;
				}

				if ((char)currentByte == searchChar)
				{
					return position + 1;
				}

				position--;
			}

			return 0;
		}

		private void Store(List<PerformanceData> data)
		{
			LogFiles.ForEach(logFile => Store(data, logFile));
		}

		private void Store(List<PerformanceData> data, LogFileInfo logFileInfo)
		{
			Directory.CreateDirectory(logFileInfo.FilePath);

			string fileName = BuildFileName(logFileInfo);
			string fullPath = Path.Combine(logFileInfo.FilePath, fileName);

			lock (_fileLock)
			{
				using (var fileStream = new FileStream(fullPath, FileMode.OpenOrCreate))
				{
					fileStream.Position = GetStartPosition(fileStream);

					using (var writer = new StreamWriter(fileStream))
					{
						string prefix = fileStream.Position == 0 ? "[" : ",";
						string postfix = "]";

						var performanceLog = new PerformanceLog
						{
							Data = data.Where(d => d != null).ToList(),
							Metadata = _metadata
						};

						if (performanceLog.Any)
						{
							writer.Write(prefix + JsonConvert.SerializeObject(performanceLog, _jsonSerializerSettings) + postfix);
						}
					}
				}
			}
		}

		private string BuildFileName(LogFileInfo logFileInfo)
		{
			var sb = new StringBuilder();

			if (IncludeDate)
			{
				sb.Append($"{DateTime.UtcNow:yyyy-MM-dd hh-mm-ss.fff}_");
			}

			sb.Append($"{logFileInfo.FileName}.json");

			return sb.ToString();
		}
	}

	/// <summary>
	/// <see cref="LogFileInfo"/> represents information about a log file, including its name and path.
	/// </summary>
	public class LogFileInfo
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LogFileInfo"/> class.
		/// </summary>
		/// <param name="fileName">Name of the file to which to log.</param>
		/// <param name="filePath">Path of the <paramref name="fileName"/>.</param>
		/// <exception cref="ArgumentException">Throws if <paramref name="fileName"/> or <paramref name="filePath"/> are null or empty.</exception>
		public LogFileInfo(string fileName, string filePath)
		{
			if (String.IsNullOrWhiteSpace(fileName))
			{
				throw new ArgumentException(nameof(fileName));
			}

			if (String.IsNullOrWhiteSpace(filePath))
			{
				throw new ArgumentException(nameof(filePath));
			}

			FileName = fileName;
			FilePath = filePath;
		}

		/// <summary>
		/// Gets name of the file.
		/// </summary>
		public string FileName { get; private set; }

		/// <summary>
		/// Gets path of the <see cref="FileName"/>.
		/// </summary>
		public string FilePath { get; private set; }
	}

	internal class PerformanceLog
	{
		[JsonProperty(Order = 0)]
		public IReadOnlyDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

		[JsonProperty(Order = 1)]
		public IReadOnlyList<PerformanceData> Data { get; set; } = new List<PerformanceData>();

		[JsonIgnore]
		public bool Any => (Metadata?.Any() == true) || (Data?.Any() == true);

		public bool ShouldSerializeMetadata()
		{
			return Metadata.Count > 0;
		}
	}
}