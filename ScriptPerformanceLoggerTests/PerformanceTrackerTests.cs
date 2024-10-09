namespace ScriptPerformanceLoggerTests
{
	using System;
	using System.Linq;
	using System.Threading;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.Utils.ScriptPerformanceLogger;
	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Loggers;
	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Models;

	[TestClass]
	public class PerformanceTrackerTests
	{
		[TestMethod]
		public void PerformanceTracker_InitializedWithStartNowTrue_ShouldAutoStart()
		{
			// Arrange
			PerformanceTracker tracker = new PerformanceTracker();

			// Act
			var trackedMethod = tracker.TrackedMethod;

			// Assert
			Assert.IsNotNull(trackedMethod);
			Assert.AreEqual("PerformanceTrackerTests", trackedMethod.ClassName);
			Assert.AreEqual("PerformanceTracker_InitializedWithStartNowTrue_ShouldAutoStart", trackedMethod.MethodName);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void PerformanceTracker_InitializedWithStartNowFalse_ShouldNotAutoStart()
		{
			// Arrange
			PerformanceTracker tracker = new PerformanceTracker(false);

			// Act
			_ = tracker.TrackedMethod;

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		public void PerformanceTracker_InitializedWithCollector_ShouldAssignCollector()
		{
			// Arrange
			PerformanceCollector collector = new PerformanceCollector(new PerformanceFileLogger());

			// Act
			PerformanceTracker tracker = new PerformanceTracker(collector);

			// Assert
			Assert.AreEqual(collector, tracker.Collector);
		}

		[TestMethod]
		public void PerformanceTracker_InitializedWithPerformanceTracker_ShouldAssignCorrectParent()
		{
			// Arrange
			PerformanceTracker parentTracker = new PerformanceTracker();

			// Act
			PerformanceTracker tracker = new PerformanceTracker(parentTracker);

			// Assert
			Assert.AreEqual(parentTracker.TrackedMethod, tracker.TrackedMethod.Parent);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void PerformanceTracker_InitializedWithPerformanceTracker_ShouldThrowIfParentIsNotStarted()
		{
			// Arrange
			PerformanceTracker parentTracker = new PerformanceTracker(false);

			// Act
			_ = new PerformanceTracker(parentTracker);

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		public void PerformanceTracker_InitializedWithPerformanceTracker_ShouldNotAddMethodToParentsSubMethodOnTheSameThread()
		{
			// Arrange
			PerformanceTracker parentTracker = new PerformanceTracker();

			// Act
			PerformanceTracker tracker = new PerformanceTracker(parentTracker);

			// Assert
			var parentThreadIdFieldInfo = parentTracker.GetType().GetField("_threadId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			var trackerThreadIdFieldInfo = tracker.GetType().GetField("_threadId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			Assert.AreEqual(parentThreadIdFieldInfo.GetValue(parentTracker), trackerThreadIdFieldInfo.GetValue(tracker));
			Assert.AreEqual(1, parentTracker.TrackedMethod.SubMethods.Where(m => m == tracker.TrackedMethod).Count());
		}

		[TestMethod]
		public void PerformanceTracker_InitializedWithPerformanceTracker_ShouldAddMethodToParentsSubMethodForDifferentThreads()
		{
			// Arrange
			PerformanceTracker parentTracker = new PerformanceTracker();

			// Act
			PerformanceTracker tracker = new PerformanceTracker(parentTracker);

			// Simulate different thread IDs
			parentTracker.GetType()
				.GetField("_threadId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				.SetValue(parentTracker, Thread.CurrentThread.ManagedThreadId + 1);

			// Assert
			var parentThreadIdFieldInfo = parentTracker.GetType().GetField("_threadId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			var trackerThreadIdFieldInfo = tracker.GetType().GetField("_threadId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			Assert.AreNotEqual(parentThreadIdFieldInfo.GetValue(parentTracker), trackerThreadIdFieldInfo.GetValue(tracker));
			Assert.AreEqual(1, parentTracker.TrackedMethod.SubMethods.Where(m => m == tracker.TrackedMethod).Count());
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void PerformanceTracker_EndWithoutStart_ShouldThrowInvalidOperationException()
		{
			// Arrange
			PerformanceTracker tracker = new PerformanceTracker(false);

			// Act
			tracker.End();

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		public void PerformanceTracker_StartStringString_ShouldStartTracking()
		{
			// Arrange
			PerformanceTracker tracker = new PerformanceTracker(false);

			// Act
			var performanceData = tracker.Start("PerformanceTrackerTests", "PerformanceTracker_StartStringString_ShouldStartTracking");

			// Assert
			Assert.IsNotNull(performanceData);
			Assert.AreEqual("PerformanceTrackerTests", performanceData.ClassName);
			Assert.AreEqual("PerformanceTracker_StartStringString_ShouldStartTracking", performanceData.MethodName);
		}

		[TestMethod]
		public void PerformanceTracker_StartAndEnd_ShouldTrackPerformanceDataCorrectly()
		{
			// Arrange
			PerformanceTracker tracker = new PerformanceTracker(false);

			// Act
			tracker.Start();
			PerformanceData performanceData = tracker.End();

			// Assert
			Assert.IsNotNull(performanceData);
			Assert.AreEqual("PerformanceTrackerTests", performanceData.ClassName);
			Assert.AreEqual("PerformanceTracker_StartAndEnd_ShouldTrackPerformanceDataCorrectly", performanceData.MethodName);
			Assert.AreNotEqual(default(DateTime), performanceData.StartTime);
			Assert.AreNotEqual(default(TimeSpan), performanceData.ExecutionTime);
		}

		[TestMethod]
		public void PerformanceTracker_End_MultipleEndShouldNotModifyTheExecutionTime()
		{
			// Arrange
			PerformanceTracker tracker = new PerformanceTracker();

			// Act
			var firstEndTime = tracker.End().ExecutionTime;
			Thread.Sleep(100);
			var secondEndTime = tracker.End().ExecutionTime;

			// Assert
			Assert.AreEqual(firstEndTime, secondEndTime);
		}

		[TestMethod]
		public void PerformanceTracker_AddMetadata_ShouldIncludeMetadataInTrackedMethod()
		{
			// Arrange
			PerformanceTracker tracker = new PerformanceTracker();
			tracker.Start();

			// Act
			tracker.AddMetadata("Key1", "Value1");
			tracker.AddMetadata("Key2", "Value2");
			PerformanceData data = tracker.End();

			// Assert
			Assert.AreEqual("Value1", data.Metadata["Key1"]);
			Assert.AreEqual("Value2", data.Metadata["Key2"]);
		}

		[TestMethod]
		public void PerformanceTracker_Dispose_ShouldEndTracking()
		{
			// Arrange
			PerformanceTracker tracker = new PerformanceTracker();

			// Act
			tracker.Dispose();

			// Assert
			Assert.AreNotEqual(default(TimeSpan), tracker.TrackedMethod.ExecutionTime);
		}

		[TestMethod]
		public void PerformanceTracker_Elapsed_ShouldReturnCorrectTime()
		{
			// Arrange
			PerformanceTracker tracker = new PerformanceTracker();

			// Act
			Thread.Sleep(100);
			var elapsed = tracker.Elapsed;

			// Assert
			Assert.IsTrue((DateTime.UtcNow - elapsed).Millisecond > 100);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void PerformanceTracker_Elapsed_ShouldThrowIfTrackingIsNotStarted()
		{
			// Arrange
			PerformanceTracker tracker = new PerformanceTracker(false);

			// Act
			Thread.Sleep(100);
			_ = tracker.Elapsed;

			// Assert is handled by ExpectedException
		}
	}
}