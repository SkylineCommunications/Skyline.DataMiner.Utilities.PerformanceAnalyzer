namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger.Loggers
{
	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Models;

	public interface IPerformanceLogger
	{
		void Report(PerformanceData data);
	}
}