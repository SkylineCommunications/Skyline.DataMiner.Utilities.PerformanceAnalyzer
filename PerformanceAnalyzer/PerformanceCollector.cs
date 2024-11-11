namespace Skyline.DataMiner.Utils.PerformanceAnalyzer
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;

	using Skyline.DataMiner.Utils.PerformanceAnalyzer.Loggers;
	using Skyline.DataMiner.Utils.PerformanceAnalyzer.Models;
	using Skyline.DataMiner.Utils.PerformanceAnalyzer.Tools;

	/// <summary>
	/// <see cref="PerformanceCollector"/> collects performance metrics for methods in single or multi threaded environments.
	/// </summary>
	public sealed class PerformanceCollector : IDisposable
	{
		private readonly PerformanceClock clock;
		private readonly IPerformanceLogger logger;
		private readonly ConcurrentDictionary<int, PerformanceData> perThreadRootMethod = new ConcurrentDictionary<int, PerformanceData>();
		private readonly List<PerformanceData> methodsToLog = new List<PerformanceData>();

		private bool disposed;

		/// <summary>
		/// Initializes a new instance of the <see cref="PerformanceCollector"/> class.
		/// </summary>
		/// <param name="logger">Implementation of the <see cref="IPerformanceLogger"/>.</param>
		public PerformanceCollector(IPerformanceLogger logger)
		{
			this.logger = logger;
			clock = new PerformanceClock();
		}

		internal PerformanceClock Clock => clock;

		internal PerformanceData Start(PerformanceData methodData, int threadId)
		{
			if (methodData.IsStarted)
			{
				return methodData;
			}

			if (methodData.Parent == null)
			{
				perThreadRootMethod.TryAdd(threadId, methodData);
			}

			if (perThreadRootMethod.Count == 1)
			{
				disposed = false;
			}

			methodData.StartTime = clock.UtcNow;
			methodData.IsStarted = true;

			return methodData;
		}

		internal PerformanceData Stop(PerformanceData methodData)
		{
			if (methodData.IsStopped)
			{
				return methodData;
			}

			methodData.ExecutionTime = clock.UtcNow - methodData.StartTime;
			methodData.IsStopped = true;

			return methodData;
		}

		/// <summary>
		/// Collects data to log, when all data to log is collected executes <see cref="IPerformanceLogger.Report(List{PerformanceData})"/> with the collected data.
		/// </summary>
#pragma warning disable SA1202 // Elements should be ordered by access
		public void Dispose()
#pragma warning restore SA1202 // Elements should be ordered by access
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!disposed && disposing)
			{
				perThreadRootMethod.TryRemove(Thread.CurrentThread.ManagedThreadId, out var rootMethod);

				methodsToLog.Add(rootMethod);

				if (!perThreadRootMethod.Any())
				{
					logger.Report(methodsToLog);
					methodsToLog.Clear();

					disposed = true;
				}
			}
		}
	}
}