namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger.Loggers
{
	using System.Collections.Generic;

	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Models;

	public interface IPerformanceLogger
	{
		void Report(List<PerformanceData> data);
	}
}