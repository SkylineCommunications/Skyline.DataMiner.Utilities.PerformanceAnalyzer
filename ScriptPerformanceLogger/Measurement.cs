namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger
{
	using System;
	using System.Collections.Generic;

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

		public Dictionary<string, string> Metadata { get; private set; } = new Dictionary<string, string>();

		public void SetMetadata(string name, string value)
		{
			if (String.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace.", nameof(name));
			}

			Metadata[name] = value;
		}

		public void Dispose()
		{
			EndTime = _logger.Clock.UtcNow;

			Invocation.SetExecutionTime(StartTime, Elapsed);
			Invocation.AddMetadata(Metadata);

			_logger.CompleteMethodCallMetric(this);
		}
	}
}