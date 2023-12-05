namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger
{
	using System;
	using System.Collections.Generic;

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
			EndTime = _logCreator.Clock.UtcNow;

			Invocation.SetExecutionTime(StartTime, Elapsed);
			Invocation.AddMetadata(Metadata);

			_logCreator.CompleteMethodCallMetric(this);
		}
	}
}