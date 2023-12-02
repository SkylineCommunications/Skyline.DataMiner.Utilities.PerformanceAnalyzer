namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger
{
	using System;

	public class Measurement : IDisposable
	{
		private readonly LogCreator _logCreator;

		internal Measurement(LogCreator logCreator, MethodInvocation invocation)
		{
			_logCreator = logCreator ?? throw new ArgumentNullException(nameof(logCreator));

			Invocation = invocation ?? throw new ArgumentNullException(nameof(invocation));
			StartTime = logCreator.Clock.UtcNow;
		}

		public MethodInvocation Invocation { get; }

		public DateTime StartTime { get; }

		public DateTime EndTime { get; private set; }

		public TimeSpan Elapsed => EndTime - StartTime;

		public void Dispose()
		{
			EndTime = _logCreator.Clock.UtcNow;
			Invocation.SetExecutionTime(StartTime, Elapsed);

			_logCreator.CompleteMethodCallMetric(this);
		}
	}
}