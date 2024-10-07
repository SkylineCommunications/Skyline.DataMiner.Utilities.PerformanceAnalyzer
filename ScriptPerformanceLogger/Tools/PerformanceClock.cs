﻿namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger.Tools
{
	using System;
	using System.Diagnostics;

	/// <summary>
	/// <see cref="PerformanceClock"/> is high precision clock.
	/// </summary>
	internal class PerformanceClock
	{
		private readonly DateTime _startTime;
		private readonly Stopwatch _stopwatch;

		/// <summary>
		/// Initializes a new instance of the <see cref="PerformanceClock"/> class.
		/// </summary>
		public PerformanceClock()
		{
			_startTime = DateTime.UtcNow;
			_stopwatch = Stopwatch.StartNew();
		}

		/// <summary>
		/// Gets high precision UtcNow time.
		/// </summary>
		public DateTime UtcNow => _startTime + _stopwatch.Elapsed;
	}
}