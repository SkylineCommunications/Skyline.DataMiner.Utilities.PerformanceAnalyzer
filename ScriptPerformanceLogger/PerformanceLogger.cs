namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Runtime.CompilerServices;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Tools;

	public class PerformanceLogger : IDisposable
	{
		private static readonly string DirectoryPath = @"C:\Skyline_Data\ScriptPerformanceLogger";

		private readonly Stack<MethodInvocation> _runningMethods = new Stack<MethodInvocation>();
		private readonly HashSet<MethodInvocation> _endedMethods = new HashSet<MethodInvocation>();

		public PerformanceLogger()
		{
		}

		public PerformanceLogger(string title)
		{
			SetTitle(title);
		}

		public string Title { get; private set; }

		public Result Result { get; private set; } = new Result();

		public HighResClock Clock { get; } = new HighResClock();

		/// <summary>
		/// Gets or sets a value indicating whether the file should be written to disk. Useful for unit testing.
		/// </summary>
		public bool DisableFileWrite { get; set; } = false;

		public void SetTitle(string title)
		{
			if (String.IsNullOrWhiteSpace(title))
			{
				throw new ArgumentException($"'{nameof(title)}' cannot be null or whitespace.", nameof(title));
			}

			Title = title;
		}

		public void SetProperty(string name, string value)
		{
			if (String.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));
			}

			Result.Properties[name] = value;
		}

		// no inlining to make sure the retrieved method name is correct
		[MethodImpl(MethodImplOptions.NoInlining)]
		public Measurement StartMeasurement()
		{
			var methodBase = new StackTrace().GetFrame(1).GetMethod();
			var className = methodBase.ReflectedType?.FullName;

			return StartMeasurement(className, methodBase.Name);
		}

		public Measurement StartMeasurement(string className, string methodName)
		{
			lock (_runningMethods)
			{
				var invocation = new MethodInvocation(className, methodName);

				if (_runningMethods.Count > 0)
				{
					_runningMethods.Peek().ChildInvocations.Add(invocation);
				}
				else
				{
					Result.MethodInvocations.Add(invocation);
				}

				_runningMethods.Push(invocation);

				return new Measurement(this, invocation);
			}
		}

		internal void EndMeasurement(Measurement measurement)
		{
			lock (_runningMethods)
			{
				_endedMethods.Add(measurement.Invocation);

				while (_runningMethods.Count > 0 && _endedMethods.Contains(_runningMethods.Peek()))
				{
					var method = _runningMethods.Pop();
				}
			}
		}

		public void RegisterResult(MethodInvocation methodInvocation)
		{
			if (methodInvocation == null)
				throw new ArgumentNullException(nameof(methodInvocation));

			lock (_runningMethods)
			{
				if (_runningMethods.Count > 0)
				{
					_runningMethods.Peek().ChildInvocations.Add(methodInvocation);
				}
				else
				{
					Result.MethodInvocations.Add(methodInvocation);
				}
			}
		}

		public void RegisterResult(string className, string methodName, DateTime timeStamp, TimeSpan executionTime, IDictionary<string, string> metadata = null)
		{
			RegisterResult(new MethodInvocation(className, methodName, timeStamp, executionTime, metadata));
		}

		/// <summary>Moves results from memory to file.</summary>
		/// <exception cref="SystemException">When writing the file fails.</exception>
		public void PerformCleanUpAndStoreResult()
		{
			var result = PerformCleanupAndReturn();
			if (result == null)
			{
				return;
			}

			Retry.Execute(
				() => Store(result),
				TimeSpan.FromMilliseconds(100),
				tryCount: 10);
		}

		internal Result PerformCleanupAndReturn()
		{
			var result = Result;
			Result = new Result();
			return result;
		}

		private void Store(Result result)
		{
			if (DisableFileWrite)
			{
				return;
			}

			// TODO: get rid of old results?

			Directory.CreateDirectory(DirectoryPath);

			var fileName = $"{DateTime.UtcNow:yyyy-MM-dd hh-mm-ss.fff}_{Title ?? "Untitled"}.json";

			using (var fileStream = File.CreateText(Path.Combine(DirectoryPath, fileName)))
			{
				var jsonSerializer = new JsonSerializer
				{
					NullValueHandling = NullValueHandling.Ignore,
					ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
					DateFormatHandling = DateFormatHandling.IsoDateFormat,
				};

				jsonSerializer.Serialize(fileStream, result);
			}
		}

		#region Disposable

		public void Dispose()
		{
			PerformCleanUpAndStoreResult();
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}