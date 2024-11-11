namespace Skyline.DataMiner.Utils.PerformanceAnalyzer.Loggers
{
	using System.Collections.Generic;

	using Skyline.DataMiner.Utils.PerformanceAnalyzer.Models;

	/// <summary>
	/// Interface for reporting performance metrics.
	/// </summary>
	public interface IPerformanceLogger
	{
		/// <summary>
		/// This method will be called by <see cref="PerformanceCollector.Dispose()"/>.
		/// </summary>
		/// <param name="data">List of performance metrics collected by <see cref="PerformanceCollector"/>.</param>
		void Report(List<PerformanceData> data);
	}
}