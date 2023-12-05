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

			StartTime = _logCreator.Clock.UtcNow;
			Invocation.SetStartTime(StartTime);
		}

		public MethodInvocation Invocation { get; }

		public DateTime StartTime { get; }

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
			var endTime = _logCreator.Clock.UtcNow;
			var elapsed = endTime - StartTime;

			Invocation.SetExecutionTime(elapsed);
			Invocation.AddMetadata(Metadata);

			_logCreator.EndMeasurement(this);
		}
	}
}