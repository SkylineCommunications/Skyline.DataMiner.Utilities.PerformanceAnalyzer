namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger
{
	using System;
	using System.Collections.Generic;

	public class Measurement : IDisposable
	{
		private readonly PerformanceLogger _logger;
		private readonly DateTime _startTime;

		internal Measurement(PerformanceLogger logger, MethodInvocation invocation)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			Invocation = invocation ?? throw new ArgumentNullException(nameof(invocation));

			_startTime = logger.Clock.UtcNow;
			Invocation.SetStartTime(_startTime);
		}

		public MethodInvocation Invocation { get; }

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
			var endTime = _logger.Clock.UtcNow;
			var elapsed = endTime - _startTime;

			Invocation.SetExecutionTime(elapsed);
			Invocation.AddMetadata(Metadata);

			_logger.EndMeasurement(this);
		}
	}
}