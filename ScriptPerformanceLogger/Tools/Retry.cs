namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger.Tools
{
	using System;
	using System.Threading;

	/// <summary>
	/// <see cref="Retry"/> class provides mechanism for retrying an action.
	/// </summary>
	public static class Retry
	{
		/// <summary>
		/// Retries specified action for specified number of times.
		/// </summary>
		/// <param name="action">Action to execute.</param>
		/// <param name="sleepPeriod">Sleep duration between executions.</param>
		/// <param name="tryCount">Maximum number of attempts.</param>
		/// <exception cref="ArgumentOutOfRangeException">Throws if execution didn't succeed for <paramref name="tryCount"/> number of times.</exception>
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
					break;
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