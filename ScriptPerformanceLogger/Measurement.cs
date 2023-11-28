namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger
{
	using System;
	using System.Diagnostics;

	public class Measurement : IDisposable
	{
		private readonly PerformanceLogger _logger;
		private readonly Stopwatch _stopwatch;

		internal Measurement(PerformanceLogger logger, MethodInvocation invocation)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			Invocation = invocation ?? throw new ArgumentNullException(nameof(invocation));

			StartTime = DateTime.UtcNow;
			_stopwatch = Stopwatch.StartNew();
		}

		public MethodInvocation Invocation { get; }

		public DateTime StartTime { get; }

		public TimeSpan Elapsed => _stopwatch.Elapsed;

		public void Dispose()
		{
			_stopwatch.Stop();

			Invocation.SetExecutionTime(StartTime, Elapsed);

			_logger.CompleteMethodCallMetric(this);
		}
	}
}