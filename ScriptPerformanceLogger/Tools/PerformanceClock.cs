namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger.Tools
{
	using System;
	using System.Diagnostics;

	public class PerformanceClock
	{
		private readonly DateTime _startTime;
		private readonly Stopwatch _stopwatch;

		public PerformanceClock()
		{
			_startTime = DateTime.UtcNow;
			_stopwatch = Stopwatch.StartNew();
		}

		public TimeSpan Elapsed => _stopwatch.Elapsed;

		public DateTime UtcNow => _startTime + _stopwatch.Elapsed;
	}
}