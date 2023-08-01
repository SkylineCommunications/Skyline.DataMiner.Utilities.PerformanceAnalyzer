namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Runtime.CompilerServices;

	using Newtonsoft.Json;

	public static class PerformanceLogger
	{
		const string DirectoryPath = @"C:\Skyline_Data\ScriptPerformanceLogger";

		[ThreadStatic]
		private static LogCreator _logCreator;

		public static void SetProperty(string name, string value)
		{
			var logCreator = GetCreator();
			logCreator.Result.Properties[name] = value;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static Measurement Start()
		{
			var methodBase = new StackTrace().GetFrame(1).GetMethod();
			var className = methodBase.ReflectedType?.FullName;

			return Start(className, methodBase.Name);
		}

		public static Measurement Start(string className, string methodName)
		{
			var logCreator = GetCreator();
			return logCreator.StartMeasurement(className, methodName);
		}

		public static void RegisterResult(MethodInvocation methodInvocation)
		{
			var logCreator = GetCreator();
			logCreator.RegisterResult(methodInvocation);
		}

		public static void RegisterResult(string className, string methodName, DateTime timeStamp, TimeSpan executionTime)
		{
			RegisterResult(new MethodInvocation(className, methodName, timeStamp, executionTime));
		}

		/// <summary>
		///	Moves results from memory to file.
		/// </summary>
		/// <param name="title">Will be used to create the file name.</param>
		/// <exception cref="ArgumentException">When <paramref name="title"/> would violate file path constraints.</exception>
		/// <exception cref="SystemException">When writing the file fails.</exception>
		public static void PerformCleanUpAndStoreResult(string title)
		{
			var result = PerformCleanupAndReturn();
			if (result == null)
			{
				return;
			}

			Store(result, title);
		}

		internal static Result PerformCleanupAndReturn()
		{
			var result = _logCreator?.Result;

			_logCreator = null;

			return result;
		}

		private static void Store(Result result, string title)
		{
			// todo get rid of old results?

			Directory.CreateDirectory(DirectoryPath);

			var fileName = $"{DateTime.UtcNow:yyyy-MM-dd hh-mm-ss.fff}_{title ?? "Untitled"}.json";

			using (var fileStream = File.CreateText(Path.Combine(DirectoryPath, fileName)))
			{
				var jsonSerializer = new JsonSerializer
				{
					NullValueHandling = NullValueHandling.Include,
					ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
					DateFormatHandling = DateFormatHandling.IsoDateFormat,
				};

				jsonSerializer.Serialize(fileStream, result);
			}
		}

		private static LogCreator GetCreator()
		{
			return _logCreator ?? (_logCreator = new LogCreator());
		}
	}
}