namespace ScriptPerformanceLoggerTests.Tools
{
	using System;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Tools;

	[TestClass]
	public class RetryTests
	{
		private const int _retryCount = 3;

		[TestMethod]
		public void Execute_ActionSucceedsOnFirstTry_ShouldSucceed()
		{
			// Arrange
			bool isActionExecuted = false;

			// Act
			Retry.Execute(() => isActionExecuted = true, TimeSpan.FromMilliseconds(100), 3);

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
			Retry.Execute(
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
				_retryCount);

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
				Retry.Execute(
					() =>
						{
							attempt++;
							throw new Exception("Always fails");
						},
					TimeSpan.FromMilliseconds(100),
					_retryCount);
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
			Retry.Execute(() => { }, TimeSpan.FromMilliseconds(100), 0);
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
				Retry.Execute(
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
					_retryCount);
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