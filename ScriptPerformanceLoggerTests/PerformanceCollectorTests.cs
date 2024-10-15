namespace ScriptPerformanceLoggerTests
{
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Tasks;

	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Moq;

	using Skyline.DataMiner.Utils.ScriptPerformanceLogger;
	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Loggers;
	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Models;

	[TestClass]
	public class PerformanceCollectorTests
	{
		private Mock<IPerformanceLogger> _mockLogger;
		private PerformanceCollector _collector;

		[TestInitialize]
		public void Setup()
		{
			_mockLogger = new Mock<IPerformanceLogger>();
			_collector = new PerformanceCollector(_mockLogger.Object);
		}

		[TestMethod]
		public void PerformanceCollector_Start_ShouldSetStartTime()
		{
			// Arrange
			var methodData = new PerformanceData();

			// Act
			var result = _collector.Start(methodData, Thread.CurrentThread.ManagedThreadId);

			// Assert
			Assert.IsTrue(result.IsStarted);
			Assert.AreNotEqual(default(DateTime), result.StartTime);
		}

		[TestMethod]
		public void PerformanceCollector_Start_ShouldSetIsStarted()
		{
			// Arrange
			var methodData = new PerformanceData();

			// Act
			var result = _collector.Start(methodData, Thread.CurrentThread.ManagedThreadId);

			// Assert
			Assert.IsTrue(result.IsStarted);
		}

		[TestMethod]
		public void PerformanceCollector_Start_ShouldNotOverrideAlreadyStartedMethod()
		{
			// Arrange
			var methodData = new PerformanceData { IsStarted = true, StartTime = DateTime.UtcNow.AddSeconds(-10) };

			// Act
			var result = _collector.Start(methodData, Thread.CurrentThread.ManagedThreadId);

			// Assert
			Assert.AreEqual(methodData.StartTime, result.StartTime);
		}

		[TestMethod]
		public void PerformanceCollector_Stop_ShouldSetExecutionTime()
		{
			// Arrange
			var methodData = new PerformanceData { StartTime = DateTime.UtcNow };

			// Act
			var result = _collector.Stop(methodData);

			// Assert
			Assert.AreNotEqual(default(TimeSpan), result.ExecutionTime);
		}

		[TestMethod]
		public void PerformanceCollector_Stop_ShouldSetIsStopped()
		{
			// Arrange
			var methodData = new PerformanceData { StartTime = DateTime.UtcNow };

			// Act
			var result = _collector.Stop(methodData);

			// Assert
			Assert.IsTrue(result.IsStopped);
		}

		[TestMethod]
		public void PerformanceCollector_Stop_ShouldNotModifyAlreadyStoppedMethod()
		{
			// Arrange
			var methodData = new PerformanceData { IsStopped = true, StartTime = DateTime.UtcNow.AddSeconds(-10) };

			// Act
			var result = _collector.Stop(methodData);

			// Assert
			Assert.AreEqual(methodData.ExecutionTime, result.ExecutionTime);
		}

		[TestMethod]
		public void PerformanceCollector_Dispose_ShouldLogMethodsAndClear()
		{
			// Arrange
			var methodData = new PerformanceData();
			_collector.Start(methodData, Thread.CurrentThread.ManagedThreadId);

			// Act
			_collector.Dispose();

			// Assert
			_mockLogger.Verify(l => l.Report(It.IsAny<List<PerformanceData>>()), Times.Once);
		}

		[TestMethod]
		public void PerformanceCollector_Dispose_ShouldNotLogMethodsWhileOtherThreadsAreRunning()
		{
			// Arrange
			var methodData = new PerformanceData();
			_collector.Start(methodData, Thread.CurrentThread.ManagedThreadId);

			Task.Run(() =>
			{
				_collector.Start(new PerformanceData(), Thread.CurrentThread.ManagedThreadId);

				Thread.Sleep(200);
			});

			// Act
			Thread.Sleep(100);
			_collector.Dispose();

			// Assert
			_mockLogger.Verify(l => l.Report(It.IsAny<List<PerformanceData>>()), Times.Never);
		}
	}
}
