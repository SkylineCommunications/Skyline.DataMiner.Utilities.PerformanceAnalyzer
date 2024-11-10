namespace Skyline.DataMiner.Utilities.PerformanceAnalyzerTests.Tools
{
	using System;
	using System.Threading;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.Utilities.PerformanceAnalyzer.Tools;

	[TestClass]
	public class PerformanceClockTests
	{
		[TestMethod]
		public void PerformanceClock_UtcNow_ShouldReturnValueCloseToSystemTime()
		{
			// Arrange
			PerformanceClock clock = new PerformanceClock();

			// Act
			DateTime clockTime = clock.UtcNow;
			DateTime systemTime = DateTime.UtcNow;

			// Assert
			// Allow for a small difference due to execution time
			Assert.IsTrue((systemTime - clockTime).TotalMilliseconds < 50, "UtcNow is not close to the system UtcNow.");
		}

		[TestMethod]
		public void PerformanceClock_UtcNow_ShouldUpdateAfterDelay()
		{
			// Arrange
			PerformanceClock clock = new PerformanceClock();

			// Act
			DateTime beforeDelay = clock.UtcNow;
			Thread.Sleep(100); // Sleep for 100ms
			DateTime afterDelay = clock.UtcNow;

			// Assert
			Assert.IsTrue(afterDelay > beforeDelay, "UtcNow did not update after delay");
		}
	}
}