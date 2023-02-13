namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.CompilerServices;

	using Newtonsoft.Json;

	public static class PerformanceLogger
	{
		private static readonly ConcurrentDictionary<int, LogCreator> Storage =
			new ConcurrentDictionary<int, LogCreator>();

		public static void SetProperty(string name, string value)
		{
			LogCreator logCreator = GetCreator();
			logCreator.Result.Properties[name] = value;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static IDisposable Start()
		{
			LogCreator creator = GetCreator();
			MethodBase methodBase = new StackTrace().GetFrame(1).GetMethod();
			string className = methodBase.ReflectedType?.FullName;
			return creator.StartDisposableMethodCallMetric(className, methodBase.Name);
		}

		public static IDisposable Start(string className, string methodName)
		{
			return GetCreator().StartDisposableMethodCallMetric();
		}

		/// <summary>
		///	Moves results from memory to file.
		/// </summary>
		/// <param name="title">Will be used to create the file name.</param>
		/// <exception cref="ArgumentException">When <paramref name="title"/> would violate file path constraints.</exception>
		/// <exception cref="SystemException">When writing the file fails.</exception>
		public static void PerformCleanUpAndStoreResult(string title)
		{
			Result result = PerformCleanupAndReturn();
			if (result == null)
			{
				return;
			}

			Store(result, title);
		}

		internal static Result PerformCleanupAndReturn()
		{
			if (!TryPerformCleanup(out LogCreator creator))
			{
				return null;
			}

			return creator.Result;
		}

		private static void Store(Result result, string title)
		{
			// todo get rid of old results?
			const string DirectoryPath = @"C:\Skyline_Data\ScriptPerformanceLogger";
			Directory.CreateDirectory(DirectoryPath);
			var fileName = $"{DateTime.UtcNow.ToString("yyyy-MM-dd hh-mm-ss.fff ")}_{Path.ChangeExtension(title ?? "Untitled", "json")}";
			using (StreamWriter fileStream = File.CreateText(Path.Combine(DirectoryPath, fileName)))
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

		private static bool TryPerformCleanup(out LogCreator creator)
		{
			GetRidOfDayOldMetrics();
			return Storage.TryRemove(Environment.CurrentManagedThreadId, out creator);
		}

		private static void GetRidOfDayOldMetrics()
		{
			DateTime utcNow = DateTime.UtcNow;
			foreach (KeyValuePair<int, LogCreator> pair in Storage.ToArray())
			{
				MethodInvocation methodInvocation = pair.Value.Result.MethodInvocations.FirstOrDefault();
				if (methodInvocation == null)
				{
					continue;
				}

				if ((methodInvocation.TimeStamp - utcNow.AddDays(1)).Duration() > TimeSpan.FromDays(1))
				{
					Storage.TryRemove(pair.Key, out _);
				}
			}
		}

		private static LogCreator GetCreator()
		{
			int threadId = Environment.CurrentManagedThreadId;
			if (!Storage.TryGetValue(threadId, out LogCreator creator))
			{
				creator = new LogCreator();
				Storage.TryAdd(threadId, creator);
			}

			return creator;
		}
	}
}