namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger.Tools
{
	using System;
	using System.Diagnostics;

	public class HighResClock
	{
		private readonly DateTime _startTime;
		private readonly Stopwatch _stopwatch;

		public HighResClock()
		{
			_startTime = DateTime.UtcNow;
			_stopwatch = Stopwatch.StartNew();
		}

		public DateTime UtcNow
		{
			get
			{
				return _startTime + _stopwatch.Elapsed;
			}
		}
	}
}