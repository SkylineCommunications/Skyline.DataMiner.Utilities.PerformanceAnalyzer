namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger.Loggers
{
	using System.Collections.Generic;
	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Models;

	public class UnboundPerformanceLogger : IPerformanceLogger
	{
		private readonly List<IPerformanceLogger> _performanceLoggers = new List<IPerformanceLogger>();

		public UnboundPerformanceLogger() : this(new IPerformanceLogger[0])
		{
		}

		public UnboundPerformanceLogger(params IPerformanceLogger[] performanceLoggers)
		{
			foreach (var performanceLogger in performanceLoggers)
			{
				_performanceLoggers.Add(performanceLogger);
			}
		}

		public void Add(IPerformanceLogger performanceLogger)
		{
			_performanceLoggers.Add(performanceLogger);
		}

		public void Report(List<PerformanceData> data)
		{
			foreach (var logger in _performanceLoggers)
			{
				logger.Report(data);
			}
		}
	}
}
