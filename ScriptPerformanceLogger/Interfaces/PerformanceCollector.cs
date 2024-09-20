namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger.Interfaces
{
	using System;
	using System.Collections.Concurrent;
	using System.Threading;

	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Loggers;
	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Models;
	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Tools;

	public sealed class PerformanceCollector : IDisposable
	{
		private readonly PerformanceClock _clock;
		private readonly IPerformanceLogger _logger;
		private readonly ConcurrentDictionary<int, PerformanceData> _threadRootMethods = new ConcurrentDictionary<int, PerformanceData>();
		private bool _disposed;

		public PerformanceCollector(IPerformanceLogger logger)
		{
			_logger = logger;
			_clock = new PerformanceClock();
		}

		public TimeSpan Elapsed => _clock.Elapsed;

		public PerformanceData RootMethod => _threadRootMethods[Thread.CurrentThread.ManagedThreadId];

		public PerformanceData Start(PerformanceData methodData)
		{
			_threadRootMethods.TryAdd(Thread.CurrentThread.ManagedThreadId, methodData);

			methodData.StartTime = _clock.UtcNow;
			return methodData;
		}

		public PerformanceData Stop(PerformanceData methodData)
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
				_logger.Report(RootMethod);
			}

			_disposed = true;
		}
	}
}