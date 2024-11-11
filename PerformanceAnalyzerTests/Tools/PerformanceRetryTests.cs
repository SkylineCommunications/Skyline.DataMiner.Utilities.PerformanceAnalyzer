namespace Skyline.DataMiner.Utils.PerformanceAnalyzerTests.Tools
{
	using System;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.Utils.PerformanceAnalyzer.Tools;

	[TestClass]
	public class PerformanceRetryTests
	{
		private const int RetryCount = 3;

		[TestMethod]
		public void Execute_ActionSucceedsOnFirstTry_ShouldSucceed()
		{
			// Arrange
			bool isActionExecuted = false;

			// Act
			PerformanceRetry.Execute(() => isActionExecuted = true, TimeSpan.FromMilliseconds(100), 3);

			// Assert
			Assert.IsTrue(isActionExecuted);
		}

		[TestMethod]
		public void Execute_ActionFailsThenSucceeds_ShouldRetryAndSucceed()
		{
			// Arrange
			bool isActionExecuted = false;
			int attempt = 0;

			// Act
			PerformanceRetry.Execute(
				() =>
					{
						if (++attempt < 2)
						{
							throw new Exception("Failed on first attempt");
						}
						else
						{
							isActionExecuted = true;
						}
					},
				TimeSpan.FromMilliseconds(100),
				RetryCount);

			// Assert
			Assert.AreEqual(2, attempt);
			Assert.IsTrue(isActionExecuted);
		}

		[TestMethod]
		public void Execute_ShouldRetryForSpecifiedNumberOfTimes()
		{
			// Arrange
			int attempt = 0;

			// Act
			try
			{
				PerformanceRetry.Execute(
					() =>
						{
							attempt++;
							throw new Exception("Always fails");
						},
					TimeSpan.FromMilliseconds(100),
					RetryCount);
			}
			catch (Exception)
			{
				// Expected to throw after retries
			}

			// Assert
			Assert.AreEqual(3, attempt);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Execute_InvalidTryCount_ShouldThrowArgumentOutOfRangeException()
		{
			// Arrange & Act
			PerformanceRetry.Execute(() => { }, TimeSpan.FromMilliseconds(100), 0);

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		public void Execute_ShouldWaitForSpecifiedSleepPeriodBetweenRetries()
		{
			// Arrange
			int attempt = 0;
			TimeSpan sleepPeriod = TimeSpan.FromMilliseconds(200);
			DateTime lastAttemptTime = DateTime.Now;

			// Act
			try
			{
				PerformanceRetry.Execute(
					() =>
						{
							if (attempt > 0)
							{
								TimeSpan timeBetweenRetries = DateTime.Now - lastAttemptTime;
								Assert.IsTrue(timeBetweenRetries >= sleepPeriod);
							}

							lastAttemptTime = DateTime.Now;
							attempt++;
							throw new Exception("Always fails");
						},
					sleepPeriod,
					RetryCount);
			}
			catch (Exception)
			{
				// Expected to throw after retries
			}

			// Assert
			Assert.AreEqual(3, attempt);
		}
	}
}