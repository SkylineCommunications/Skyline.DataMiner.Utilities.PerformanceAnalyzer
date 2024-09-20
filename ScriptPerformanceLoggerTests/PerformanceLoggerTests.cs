//namespace ScriptPerformanceLoggerTests
//{
//	using System;
//	using System.Collections.Generic;
//	using System.Linq;

//	using FluentAssertions;

//	using Microsoft.VisualStudio.TestTools.UnitTesting;

//	using Skyline.DataMiner.Utils.ScriptPerformanceLogger;

//	[TestClass]
//	public class MetricCreatorTests
//	{
//		[TestMethod]
//		public void PerformanceLoggerTests_Measurements()
//		{
//			var logger = new PerformanceLogger();

//			// act
//			logger.SetProperty("Script Name", "MyTestScript");
//			logger.SetProperty("Start Time", DateTime.UtcNow.ToString("O"));

//			DoStuff(logger);

//			var result = logger.PerformCleanupAndReturn();

//			// assert
//			result.MethodInvocations.Should().HaveCount(1);
//			result.MethodInvocations[0].MethodName.Should().Be(nameof(DoStuff));
//			result.MethodInvocations[0].Metadata.Should().Contain("metadata_test", "true");
//			result.MethodInvocations[0].ChildInvocations.Single().MethodName.Should().Be(nameof(DoSomeStuff));
//		}

//		[TestMethod]
//		public void PerformanceLoggerTests_RegisterResult()
//		{
//			var logger = new PerformanceLogger();

//			// act
//			logger.SetProperty("Script Name", "MyTestScript");
//			logger.SetProperty("Start Time", DateTime.UtcNow.ToString("O"));

//			var metadata = new Dictionary<string, string> { { "metadata_test", "true" } };
//			logger.RegisterResult("ScriptPerformanceLoggerTests.MetricCreatorTests", "DoStuff", DateTime.UtcNow, TimeSpan.FromMilliseconds(30), metadata);
//			logger.RegisterResult("ScriptPerformanceLoggerTests.MetricCreatorTests", "DoSomeStuff", DateTime.UtcNow, TimeSpan.FromMilliseconds(30));

//			var result = logger.PerformCleanupAndReturn();

//			// assert
//			result.MethodInvocations.Should().HaveCount(2);
//			result.MethodInvocations[0].MethodName.Should().Be(nameof(DoStuff));
//			result.MethodInvocations[0].Metadata.Should().Contain("metadata_test", "true");
//			result.MethodInvocations[1].MethodName.Should().Be(nameof(DoSomeStuff));
//		}

//		[TestMethod]
//		public void PerformanceLoggerTests_StrangeOrder()
//		{
//			var logger = new PerformanceLogger();

//			// m1 ends before m2 (can happen in multithreaded environment)
//			var m1 = logger.StartMeasurement();
//			var m2 = logger.StartMeasurement();
//			m1.Dispose();
//			m2.Dispose();
//		}

//		public void DoStuff(PerformanceLogger logger)
//		{
//			using (var measurement = logger.StartMeasurement())
//			{
//				measurement.SetMetadata("metadata_test", "true");

//				DoSomeStuff(logger);
//			}
//		}

//		public void DoSomeStuff(PerformanceLogger logger)
//		{
//			using (logger.StartMeasurement())
//			{
//				// do nothing
//			}
//		}
//	}
//}