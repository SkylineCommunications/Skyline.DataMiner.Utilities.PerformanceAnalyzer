namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger
{
	using System;
	using System.Diagnostics;

	public class Measurement : IDisposable
	{
		private readonly LogCreator _logCreator;

		private readonly Stopwatch _stopwatch;

		internal Measurement(LogCreator logCreator, MethodInvocation invocation)
		{
			_logCreator = logCreator ?? throw new ArgumentNullException(nameof(logCreator));

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

			_logCreator.CompleteMethodCallMetric(this);
		}
	}
}