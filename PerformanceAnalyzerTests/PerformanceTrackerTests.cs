﻿namespace Skyline.DataMiner.Utils.PerformanceAnalyzerTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Threading;

	using FluentAssertions;

	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Moq;

	using Skyline.DataMiner.Utils.PerformanceAnalyzer;
	using Skyline.DataMiner.Utils.PerformanceAnalyzer.Loggers;

	[TestClass]
	public class PerformanceTrackerTests
	{
		private Mock<IPerformanceLogger> mockLogger;
		private PerformanceCollector collector;
		private PerformanceTracker tracker;

		[TestInitialize]
		public void Setup()
		{
			mockLogger = new Mock<IPerformanceLogger>();
			collector = new PerformanceCollector(mockLogger.Object);
			tracker = new PerformanceTracker(collector);
		}

		[TestMethod]
		public void PerformanceTracker_Initialize_ShouldTrackMethod()
		{
			// Arrange
			PerformanceTracker tracker = new PerformanceTracker(collector);

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
			PerformanceCollector collector = new PerformanceCollector(new PerformanceFileLogger("Collection1"));

			// Act
			PerformanceTracker tracker = new PerformanceTracker(collector);

			// Assert
			Assert.AreEqual(collector, tracker.Collector);
		}

		[TestMethod]
		public void PerformanceTracker_InitializedWithCollectorAndNames_ShouldAssignCollectorAndNames()
		{
			// Arrange & Act
			PerformanceTracker tracker = new PerformanceTracker(collector, "className", "methodName");

			// Assert
			Assert.AreEqual(collector, tracker.Collector);
			Assert.AreEqual("className", tracker.TrackedMethod.ClassName);
			Assert.AreEqual("methodName", tracker.TrackedMethod.MethodName);
		}

		[TestMethod]
		public void PerformanceTracker_InitializedWithTrackerAndNames_ShouldAssignCollectorAndNames()
		{
			// Arrange & Act
			PerformanceTracker tracker = new PerformanceTracker(this.tracker, "className", "methodName");

			// Assert
			Assert.AreEqual(collector, tracker.Collector);
			Assert.AreEqual("className", tracker.TrackedMethod.ClassName);
			Assert.AreEqual("methodName", tracker.TrackedMethod.MethodName);
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
		public void PerformanceTracker_InitializedCollectorWithNullWithNames_ShouldThrow()
		{
			// Arrange
			PerformanceCollector collector = null;

			// Act
			_ = new PerformanceTracker(collector, "className", "methodName");

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PerformanceTracker_InitializedCollectorClassNameWithNull_ShouldThrow()
		{
			// Arrange & Act
			_ = new PerformanceTracker(collector, null, "methodName");

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PerformanceTracker_InitializedCollectorClassNameWithEmpty_ShouldThrow()
		{
			// Arrange & Act
			_ = new PerformanceTracker(collector, string.Empty, "methodName");

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PerformanceTracker_InitializedCollectorClassNameWithWhitespace_ShouldThrow()
		{
			// Arrange & Act
			_ = new PerformanceTracker(collector, "    ", "methodName");

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PerformanceTracker_InitializedCollectorMethodNameWithNull_ShouldThrow()
		{
			// Arrange & Act
			_ = new PerformanceTracker(collector, "className", null);

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PerformanceTracker_InitializedCollectorMethodNameWithEmpty_ShouldThrow()
		{
			// Arrange & Act
			_ = new PerformanceTracker(collector, "className", string.Empty);

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PerformanceTracker_InitializedCollectorMethodNameWithWhitespace_ShouldThrow()
		{
			// Arrange & Act
			_ = new PerformanceTracker(collector, "className", "    ");

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PerformanceTracker_InitializedParentTrackerWithNull_ShouldThrow()
		{
			// Arrange
			PerformanceTracker tracker = null;

			// Act
			_ = new PerformanceTracker(tracker);

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PerformanceTracker_InitializedParentTrackerWithNullWithNames_ShouldThrow()
		{
			// Arrange
			PerformanceTracker tracker = null;

			// Act
			_ = new PerformanceTracker(tracker, "className", "methodName");

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PerformanceTracker_InitializedParentTrackerClassNameWithNull_ShouldThrow()
		{
			// Arrange & Act
			_ = new PerformanceTracker(tracker, null, "methodName");

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PerformanceTracker_InitializedParentTrackerClassNameWithEmpty_ShouldThrow()
		{
			// Arrange & Act
			_ = new PerformanceTracker(tracker, string.Empty, "methodName");

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PerformanceTracker_InitializedParentTrackerClassNameWithWhitespace_ShouldThrow()
		{
			// Arrange & Act
			_ = new PerformanceTracker(tracker, "    ", "methodName");

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PerformanceTracker_InitializedParentTrackerMethodNameWithNull_ShouldThrow()
		{
			// Arrange & Act
			_ = new PerformanceTracker(tracker, "className", null);

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PerformanceTracker_InitializedParentTrackerMethodNameWithEmpty_ShouldThrow()
		{
			// Arrange & Act
			_ = new PerformanceTracker(tracker, "className", string.Empty);

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void PerformanceTracker_InitializedParentTrackerMethodNameWithWhitespace_ShouldThrow()
		{
			// Arrange & Act
			_ = new PerformanceTracker(tracker, "className", "    ");

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		public void PerformanceTracker_InitializedWithPerformanceTracker_ShouldAssignCorrectParent()
		{
			// Arrange
			PerformanceTracker parentTracker = new PerformanceTracker(collector);

			// Act
			PerformanceTracker tracker = new PerformanceTracker(parentTracker);

			// Assert
			Assert.AreEqual(parentTracker.TrackedMethod, tracker.TrackedMethod.Parent);
		}

		[TestMethod]
		public void PerformanceTracker_InitializedWithPerformanceTracker_ShouldAssignCorrectSubMethod()
		{
			// Arrange & Act
			PerformanceTracker tracker = new PerformanceTracker(this.tracker);

			// Assert
			Assert.IsTrue(this.tracker.TrackedMethod.SubMethodsConcurrent.Contains(tracker.TrackedMethod));
		}

		[TestMethod]
		public void PerformanceTracker_InitializedWithPerformanceTrackerWithNames_ShouldAssignCorrectParent()
		{
			// Arrange & Act
			PerformanceTracker tracker = new PerformanceTracker(this.tracker, "className", "methodName");

			// Assert
			Assert.AreEqual(this.tracker.TrackedMethod, tracker.TrackedMethod.Parent);
		}

		[TestMethod]
		public void PerformanceTracker_InitializedWithPerformanceTrackerWithNames_ShouldAssignCorrectSubMethod()
		{
			// Arrange & Act
			PerformanceTracker tracker = new PerformanceTracker(this.tracker, "className", "methodName");

			// Assert
			Assert.IsTrue(this.tracker.TrackedMethod.SubMethodsConcurrent.Contains(tracker.TrackedMethod));
		}

		[TestMethod]
		public void PerformanceTracker_InitializedWithPerformanceTracker_ShouldNotAddMethodToParentsSubMethodOnTheSameThread()
		{
			// Arrange
			PerformanceTracker parentTracker = new PerformanceTracker(collector);

			// Act
			PerformanceTracker tracker = new PerformanceTracker(parentTracker);

			// Assert
			var parentThreadIdFieldInfo = parentTracker.GetType().GetField("threadId", BindingFlags.NonPublic | BindingFlags.Instance);
			var trackerThreadIdFieldInfo = tracker.GetType().GetField("threadId", BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.AreEqual(parentThreadIdFieldInfo.GetValue(parentTracker), trackerThreadIdFieldInfo.GetValue(tracker));
			Assert.AreEqual(1, parentTracker.TrackedMethod.SubMethodsConcurrent.Where(m => m == tracker.TrackedMethod).Count());
		}

		[TestMethod]
		public void PerformanceTracker_InitializedWithPerformanceTrackerAndNames_ShouldNotAddMethodToParentsSubMethodOnTheSameThread()
		{
			// Arrange
			PerformanceTracker parentTracker = new PerformanceTracker(collector);

			// Act
			PerformanceTracker tracker = new PerformanceTracker(parentTracker, "className", "methodName");

			// Assert
			var parentThreadIdFieldInfo = parentTracker.GetType().GetField("threadId", BindingFlags.NonPublic | BindingFlags.Instance);
			var trackerThreadIdFieldInfo = tracker.GetType().GetField("threadId", BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.AreEqual(parentThreadIdFieldInfo.GetValue(parentTracker), trackerThreadIdFieldInfo.GetValue(tracker));
			Assert.AreEqual(1, parentTracker.TrackedMethod.SubMethodsConcurrent.Where(m => m == tracker.TrackedMethod).Count());
		}

		[TestMethod]
		public void PerformanceTracker_InitializedWithPerformanceTracker_ShouldAddMethodToParentsSubMethodForDifferentThreads()
		{
			// Arrange
			PerformanceTracker parentTracker = new PerformanceTracker(collector);

			// Act
			PerformanceTracker tracker = new PerformanceTracker(parentTracker);

			// Simulate different thread IDs
			parentTracker.GetType()
				.GetField("threadId", BindingFlags.NonPublic | BindingFlags.Instance)
				.SetValue(parentTracker, Thread.CurrentThread.ManagedThreadId + 1);

			// Assert
			var parentThreadIdFieldInfo = parentTracker.GetType().GetField("threadId", BindingFlags.NonPublic | BindingFlags.Instance);
			var trackerThreadIdFieldInfo = tracker.GetType().GetField("threadId", BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.AreNotEqual(parentThreadIdFieldInfo.GetValue(parentTracker), trackerThreadIdFieldInfo.GetValue(tracker));
			Assert.AreEqual(1, parentTracker.TrackedMethod.SubMethodsConcurrent.Where(m => m == tracker.TrackedMethod).Count());
		}

		[TestMethod]
		public void PerformanceTracker_InitializedWithPerformanceTrackerAndNames_ShouldAddMethodToParentsSubMethodForDifferentThreads()
		{
			// Arrange
			PerformanceTracker parentTracker = new PerformanceTracker(collector);

			// Act
			PerformanceTracker tracker = new PerformanceTracker(parentTracker, "className", "methodName");

			// Simulate different thread IDs
			parentTracker.GetType()
				.GetField("threadId", BindingFlags.NonPublic | BindingFlags.Instance)
				.SetValue(parentTracker, Thread.CurrentThread.ManagedThreadId + 1);

			// Assert
			var parentThreadIdFieldInfo = parentTracker.GetType().GetField("threadId", BindingFlags.NonPublic | BindingFlags.Instance);
			var trackerThreadIdFieldInfo = tracker.GetType().GetField("threadId", BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.AreNotEqual(parentThreadIdFieldInfo.GetValue(parentTracker), trackerThreadIdFieldInfo.GetValue(tracker));
			Assert.AreEqual(1, parentTracker.TrackedMethod.SubMethodsConcurrent.Where(m => m == tracker.TrackedMethod).Count());
		}

		[TestMethod]
		public void PerformanceTracker_InitializedWithPerformanceTracker_ShouldNotAddMethodToMutipleSubmethods()
		{
			// Arrange
			PerformanceTracker parentTracker = new PerformanceTracker(collector);
			PerformanceTracker middleTracker;
			PerformanceTracker childTracker;

			// Act
			using (middleTracker = new PerformanceTracker(parentTracker))
			{
				using (childTracker = new PerformanceTracker(parentTracker))
				{
				}
			}

			// Assert
			Assert.IsFalse(parentTracker.TrackedMethod.SubMethodsConcurrent.Contains(childTracker.TrackedMethod));
			Assert.IsTrue(middleTracker.TrackedMethod.SubMethodsConcurrent.Contains(childTracker.TrackedMethod));
		}

		[TestMethod]
		public void PerformanceTracker_InitializedWithPerformanceTrackerAndNames_ShouldNotAddMethodToMutipleSubmethods()
		{
			// Arrange
			PerformanceTracker parentTracker = new PerformanceTracker(collector);
			PerformanceTracker middleTracker;
			PerformanceTracker childTracker;

			// Act
			using (middleTracker = new PerformanceTracker(parentTracker, "className", "methodName"))
			{
				using (childTracker = new PerformanceTracker(parentTracker))
				{
				}
			}

			// Assert
			Assert.IsFalse(parentTracker.TrackedMethod.SubMethodsConcurrent.Contains(childTracker.TrackedMethod));
			Assert.IsTrue(middleTracker.TrackedMethod.SubMethodsConcurrent.Contains(childTracker.TrackedMethod));
		}

		[TestMethod]
		public void PerformanceTracker_InitializedWithPerformanceTracker_ShouldNotAddMethodToMutipleSubmethodsMultithread()
		{
			// Arrange
			PerformanceTracker parentTracker = new PerformanceTracker(collector);
			PerformanceTracker middleTracker;
			PerformanceTracker childTracker;

			// Simulate different thread IDs
			parentTracker.GetType()
				.GetField("threadId", BindingFlags.NonPublic | BindingFlags.Instance)
				.SetValue(parentTracker, Thread.CurrentThread.ManagedThreadId + 1);

			// Act
			using (middleTracker = new PerformanceTracker(parentTracker))
			{
				using (childTracker = new PerformanceTracker(parentTracker))
				{
				}
			}

			// Assert
			Assert.IsFalse(parentTracker.TrackedMethod.SubMethodsConcurrent.Contains(childTracker.TrackedMethod));
			Assert.IsTrue(middleTracker.TrackedMethod.SubMethodsConcurrent.Contains(childTracker.TrackedMethod));
		}

		[TestMethod]
		public void PerformanceTracker_InitializedWithPerformanceTrackerAndNames_ShouldNotAddMethodToMutipleSubmethodsMultithread()
		{
			// Arrange
			PerformanceTracker parentTracker = new PerformanceTracker(collector);
			PerformanceTracker middleTracker;
			PerformanceTracker childTracker;

			// Simulate different thread IDs
			parentTracker.GetType()
				.GetField("threadId", BindingFlags.NonPublic | BindingFlags.Instance)
				.SetValue(parentTracker, Thread.CurrentThread.ManagedThreadId + 1);

			// Act
			using (middleTracker = new PerformanceTracker(parentTracker, "className", "methodName"))
			{
				using (childTracker = new PerformanceTracker(parentTracker))
				{
				}
			}

			// Assert
			Assert.IsFalse(parentTracker.TrackedMethod.SubMethodsConcurrent.Contains(childTracker.TrackedMethod));
			Assert.IsTrue(middleTracker.TrackedMethod.SubMethodsConcurrent.Contains(childTracker.TrackedMethod));
		}

		[TestMethod]
		public void PerformanceTracker_AddMetadata_ShouldIncludeMetadataInTrackedMethod()
		{
			// Arrange
			PerformanceTracker tracker = new PerformanceTracker(collector);

			// Act
			tracker.AddMetadata("Key1", "Value1")
				.AddMetadata("Key2", "Value2")
				.Dispose();

			// Assert
			Assert.AreEqual("Value1", tracker.TrackedMethod.MetadataConcurrent["Key1"]);
			Assert.AreEqual("Value2", tracker.TrackedMethod.MetadataConcurrent["Key2"]);
		}

		[TestMethod]
		public void PerformanceFileLogger_AddMetadata_ShouldAddMetadataToDictionary()
		{
			// Arrange
			PerformanceTracker tracker = new PerformanceTracker(collector);
			var metadata = new Dictionary<string, string>
			{
				{ "key1", "value1" },
				{ "key2", "value2" },
			};

			// Act
			tracker.AddMetadata(metadata);

			// Assert
			Assert.AreEqual(2, tracker.TrackedMethod.MetadataConcurrent.Count);
			Assert.AreEqual("value1", tracker.TrackedMethod.MetadataConcurrent["key1"]);
			Assert.AreEqual("value2", tracker.TrackedMethod.MetadataConcurrent["key2"]);
		}

		[TestMethod]
		public void PerformanceTracker_Dispose_ShouldEndTracking()
		{
			// Arrange
			PerformanceTracker tracker = new PerformanceTracker(collector);

			// Act
			tracker.Dispose();

			// Assert
			Assert.AreNotEqual(default(TimeSpan), tracker.TrackedMethod.ExecutionTime);
		}
	}
}