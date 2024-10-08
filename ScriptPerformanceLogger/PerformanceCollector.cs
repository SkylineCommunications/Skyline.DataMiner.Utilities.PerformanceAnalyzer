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
		private bool _isStarted;
		private bool _isStopped;

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

		/// <summary>
		/// Updates start time for the method.
		/// </summary>
		/// <param name="methodData"><see cref="PerformanceData"/> of the method to update.</param>
		/// <returns>Returns updated method.</returns>
		public PerformanceData Start(PerformanceData methodData)
		{
			return Start(methodData, Thread.CurrentThread.ManagedThreadId);
		}

		/// <summary>
		/// Updates execution time of the method.
		/// </summary>
		/// <param name="methodData"><see cref="PerformanceData"/> of the method to update.</param>
		/// <returns>Returns updated method.</returns>
		public PerformanceData Stop(PerformanceData methodData)
		{
			if (_isStopped)
			{
				return methodData;
			}

			methodData.ExecutionTime = _clock.UtcNow - methodData.StartTime;
			_isStopped = true;

			return methodData;
		}

		internal PerformanceData Start(PerformanceData methodData, int threadId)
		{
			if (_isStarted)
			{
				return methodData;
			}

			if (methodData.Parent == null)
			{
				_threadRootMethods.TryAdd(threadId, methodData);
			}

			if (_threadRootMethods.Count == 1)
			{
				_disposed = false;
			}

			methodData.StartTime = _clock.UtcNow;
			_isStarted = true;

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