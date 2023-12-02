namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger
{
	using System;

	public class Measurement : IDisposable
	{
		private readonly PerformanceLogger _logger;

		internal Measurement(PerformanceLogger logger, MethodInvocation invocation)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			Invocation = invocation ?? throw new ArgumentNullException(nameof(invocation));

			StartTime = logger.Clock.UtcNow;
		}

		public MethodInvocation Invocation { get; }

		public DateTime StartTime { get; }

		public DateTime EndTime { get; private set; }

		public TimeSpan Elapsed => EndTime - StartTime;

		public void Dispose()
		{
			EndTime = _logger.Clock.UtcNow;
			Invocation.SetExecutionTime(StartTime, Elapsed);

			_logger.CompleteMethodCallMetric(this);
		}
	}
}