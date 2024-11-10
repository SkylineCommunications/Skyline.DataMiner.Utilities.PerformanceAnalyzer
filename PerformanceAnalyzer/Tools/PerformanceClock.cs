namespace Skyline.DataMiner.Utilities.PerformanceAnalyzer.Tools
{
	using System;
	using System.Diagnostics;

	/// <summary>
	/// <see cref="PerformanceClock"/> is high precision clock.
	/// </summary>
	internal class PerformanceClock
	{
		private readonly DateTime startTime;
		private readonly Stopwatch stopwatch;

		/// <summary>
		/// Initializes a new instance of the <see cref="PerformanceClock"/> class.
		/// </summary>
		public PerformanceClock()
		{
			startTime = DateTime.UtcNow;
			stopwatch = Stopwatch.StartNew();
		}

		/// <summary>
		/// Gets high precision UtcNow time.
		/// </summary>
		public DateTime UtcNow => startTime + stopwatch.Elapsed;
	}
}