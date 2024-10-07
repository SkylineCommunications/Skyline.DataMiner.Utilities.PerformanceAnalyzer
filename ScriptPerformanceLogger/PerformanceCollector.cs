namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;

	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Loggers;
	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Models;
	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Tools;

	/// <summary>
	/// <see cref="PerformanceCollector"/> collects performance metrics for methods in single or multi threaded environments.
	/// </summary>
	public sealed class PerformanceCollector : IDisposable
	{
		private readonly PerformanceClock _clock;
		private readonly IPerformanceLogger _logger;
		private readonly ConcurrentDictionary<int, PerformanceData> _threadRootMethods = new ConcurrentDictionary<int, PerformanceData>();
		private readonly List<PerformanceData> _methodsToLog = new List<PerformanceData>();

		private bool _disposed;

		/// <summary>
		/// Initializes a new instance of the <see cref="PerformanceCollector"/> class.
		/// </summary>
		/// <param name="logger">Implementation of <see cref="IPerformanceLogger"/>.</param>
		public PerformanceCollector(IPerformanceLogger logger)
		{
			_logger = logger;
			_clock = new PerformanceClock();
		}

		internal PerformanceClock Clock => _clock;

		internal PerformanceData Start(PerformanceData methodData, int threadId)
		{
			if (methodData.Parent == null)
			{
				_threadRootMethods.TryAdd(threadId, methodData);
			}

			if (_threadRootMethods.Count == 1)
			{
				_disposed = false;
			}

			methodData.StartTime = _clock.UtcNow;
			return methodData;
		}

		internal PerformanceData Stop(PerformanceData methodData)
		{
			methodData.ExecutionTime = _clock.UtcNow - methodData.StartTime;

			return methodData;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!_disposed && disposing)
			{
				_threadRootMethods.TryRemove(Thread.CurrentThread.ManagedThreadId, out var rootMethod);

				_methodsToLog.Add(rootMethod);

				if (!_threadRootMethods.Any())
				{
					_logger.Report(_methodsToLog);
					_methodsToLog.Clear();

					_disposed = true;
				}
			}
		}
	}
}