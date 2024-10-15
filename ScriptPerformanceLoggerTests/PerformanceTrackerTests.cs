namespace ScriptPerformanceLoggerTests
{
	using System;
	using System.Linq;
	using System.Threading;

	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Moq;

	using Skyline.DataMiner.Utils.ScriptPerformanceLogger;
	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Loggers;

	[TestClass]
	public class PerformanceTrackerTests
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
		public void PerformanceTracker_Initialize_ShouldTrackMethod()
		{
			// Arrange
			PerformanceTracker tracker = new PerformanceTracker(_collector);

			// Act
			var trackedMethod = tracker.TrackedMethod;

			// Assert
			Assert.IsNotNull(trackedMethod);
			Assert.AreEqual("PerformanceTrackerTests", trackedMethod.ClassName);
			Assert.AreEqual("PerformanceTracker_Initialize_ShouldTrackMethod", trackedMethod.MethodName);
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
		[ExpectedException(typeof(ArgumentNullException))]
		public void PerformanceTracker_InitializedCollectorWithNull_ShouldThrow()
		{
			// Arrange
			PerformanceCollector collector = null;

			// Act
			_ = new PerformanceTracker(collector);

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PerformanceTracker_InitializedClassNameWithNull_ShouldThrow()
		{
			// Arrange & Act
			_ = new PerformanceTracker(_collector, null, "methodName");

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PerformanceTracker_InitializedClassNameWithEmpty_ShouldThrow()
		{
			// Arrange & Act
			_ = new PerformanceTracker(_collector, string.Empty, "methodName");

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PerformanceTracker_InitializedClassNameWithWhitespace_ShouldThrow()
		{
			// Arrange & Act
			_ = new PerformanceTracker(_collector, "    ", "methodName");

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PerformanceTracker_InitializedMethodNameWithNull_ShouldThrow()
		{
			// Arrange & Act
			_ = new PerformanceTracker(_collector, "className", null);

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PerformanceTracker_InitializedMethodNameWithEmpty_ShouldThrow()
		{
			// Arrange & Act
			_ = new PerformanceTracker(_collector, "className", string.Empty);

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PerformanceTracker_InitializedMethodNameWithWhitespace_ShouldThrow()
		{
			// Arrange & Act
			_ = new PerformanceTracker(_collector, "className", "    ");

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		public void PerformanceTracker_InitializedWithPerformanceTracker_ShouldAssignCorrectParent()
		{
			// Arrange
			PerformanceTracker parentTracker = new PerformanceTracker(_collector);

			// Act
			PerformanceTracker tracker = new PerformanceTracker(parentTracker);

			// Assert
			Assert.AreEqual(parentTracker.TrackedMethod, tracker.TrackedMethod.Parent);
		}

		[TestMethod]
		public void PerformanceTracker_InitializedWithPerformanceTracker_ShouldNotAddMethodToParentsSubMethodOnTheSameThread()
		{
			// Arrange
			PerformanceTracker parentTracker = new PerformanceTracker(_collector);

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
			PerformanceTracker parentTracker = new PerformanceTracker(_collector);

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
		public void PerformanceTracker_AddMetadata_ShouldIncludeMetadataInTrackedMethod()
		{
			// Arrange
			PerformanceTracker tracker = new PerformanceTracker(_collector);

			// Act
			tracker.AddMetadata("Key1", "Value1");
			tracker.AddMetadata("Key2", "Value2");
			tracker.Dispose();

			// Assert
			Assert.AreEqual("Value1", tracker.TrackedMethod.Metadata["Key1"]);
			Assert.AreEqual("Value2", tracker.TrackedMethod.Metadata["Key2"]);
		}

		[TestMethod]
		public void PerformanceTracker_Dispose_ShouldEndTracking()
		{
			// Arrange
			PerformanceTracker tracker = new PerformanceTracker(_collector);

			// Act
			tracker.Dispose();

			// Assert
			Assert.AreNotEqual(default(TimeSpan), tracker.TrackedMethod.ExecutionTime);
		}

		[TestMethod]
		public void PerformanceTracker_Elapsed_ShouldReturnCorrectTime()
		{
			// Arrange
			PerformanceTracker tracker = new PerformanceTracker(_collector);

			// Act
			Thread.Sleep(100);
			var elapsed = tracker.Elapsed;

			// Assert
			Assert.IsTrue((DateTime.UtcNow - elapsed).Millisecond >= 100);
		}
	}
}