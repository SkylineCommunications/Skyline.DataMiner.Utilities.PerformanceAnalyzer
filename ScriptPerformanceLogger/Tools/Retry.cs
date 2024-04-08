namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger.Tools
{
	using System;
	using System.Threading;

	public static class Retry
	{
		public static void Execute(Action action, TimeSpan sleepPeriod, int tryCount = 3)
		{
			if (tryCount <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(tryCount));
			}

			while (true)
			{
				try
				{
					action();
					break; // success!
				}
				catch
				{
					if (--tryCount == 0)
					{
						throw;
					}

					Thread.Sleep(sleepPeriod);
				}
			}
		}
	}
}
