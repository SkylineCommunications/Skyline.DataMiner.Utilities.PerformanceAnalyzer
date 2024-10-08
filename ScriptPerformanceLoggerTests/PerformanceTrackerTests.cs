namespace ScriptPerformanceLoggerTests
{
	using System;
	using System.Collections.Generic;

	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Moq;

	using Skyline.DataMiner.Utils.ScriptPerformanceLogger;
	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Models;

	[TestClass]
	public class PerformanceTrackerTests
	{
		[TestMethod]
		public void PerformanceTracker_InitializedWithStartNowTrue_ShouldAutoStart()
		{
			// Arrange
			PerformanceTracker tracker = new PerformanceTracker(true);

			// Act
			var trackedMethod = tracker.TrackedMethod;

			// Assert
			Assert.IsNotNull(trackedMethod, "Tracked method should not be null when started.");
			Assert.AreEqual("PerformanceTrackerTests", trackedMethod.ClassName, "Class name should be correct.");
			Assert.AreEqual("PerformanceTracker_InitializedWithStartNowTrue_ShouldAutoStart", trackedMethod.MethodName, "Method name should be correct.");
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void PerformanceTracker_InitializedWithStartNowFalse_ShouldNotAutoStart()
		{
			// Arrange
			PerformanceTracker tracker = new PerformanceTracker(false);

			// Act
			var trackedMethod = tracker.TrackedMethod;

			// Assert is handled by ExpectedException
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
		public void PerformanceTracker_StartAndEnd_ShouldTrackPerformanceDataCorrectly()
		{
			// Arrange
			PerformanceTracker tracker = new PerformanceTracker(false);

			// Act
			tracker.Start();
			PerformanceData performanceData = tracker.End();

			// Assert
			Assert.IsNotNull(performanceData, "PerformanceData should not be null.");
			Assert.AreEqual("PerformanceTrackerTests", performanceData.ClassName, "Class name should match.");
			Assert.AreEqual("PerformanceTracker_StartAndEnd_ShouldTrackPerformanceDataCorrectly", performanceData.MethodName, "Method name should match.");
			Assert.AreNotEqual(default(DateTime), performanceData.StartTime);
			Assert.AreNotEqual(default(TimeSpan), performanceData.ExecutionTime);
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
			PerformanceTracker tracker = new PerformanceTracker(startNow: true);

			// Act
			tracker.Dispose();

			// Assert
			Assert.AreNotEqual(default(TimeSpan), tracker.TrackedMethod.ExecutionTime);
		}
	}
}