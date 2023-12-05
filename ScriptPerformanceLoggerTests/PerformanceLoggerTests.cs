namespace ScriptPerformanceLoggerTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using FluentAssertions;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.Utils.ScriptPerformanceLogger;

	[TestClass]
	public class MetricCreatorTests
	{
		[TestMethod]
		public void PerformanceLoggerTests_Measurements()
		{
			// act
			PerformanceLogger.SetProperty("Script Name", "MyTestScript");
			PerformanceLogger.SetProperty("Start Time", DateTime.UtcNow.ToString("O"));

			DoStuff();

			var result = PerformanceLogger.PerformCleanupAndReturn();

			// assert
			result.MethodInvocations.Should().HaveCount(1);
			result.MethodInvocations[0].MethodName.Should().Be(nameof(DoStuff));
			result.MethodInvocations[0].Metadata.Should().Contain("metadata_test", "true");
			result.MethodInvocations[0].ChildInvocations.Single().MethodName.Should().Be(nameof(DoSomeStuff));
		}

		[TestMethod]
		public void PerformanceLoggerTests_RegisterResult()
		{
			// act
			PerformanceLogger.SetProperty("Script Name", "MyTestScript");
			PerformanceLogger.SetProperty("Start Time", DateTime.UtcNow.ToString("O"));

			var metadata = new Dictionary<string, string> { { "metadata_test", "true" } };
			PerformanceLogger.RegisterResult("ScriptPerformanceLoggerTests.MetricCreatorTests", "DoStuff", DateTime.UtcNow, TimeSpan.FromMilliseconds(30), metadata);
			PerformanceLogger.RegisterResult("ScriptPerformanceLoggerTests.MetricCreatorTests", "DoSomeStuff", DateTime.UtcNow, TimeSpan.FromMilliseconds(30));

			var result = PerformanceLogger.PerformCleanupAndReturn();

			// assert
			result.MethodInvocations.Should().HaveCount(2);
			result.MethodInvocations[0].MethodName.Should().Be(nameof(DoStuff));
			result.MethodInvocations[0].Metadata.Should().Contain("metadata_test", "true");
			result.MethodInvocations[1].MethodName.Should().Be(nameof(DoSomeStuff));
		}

		[TestMethod]
		public void PerformanceLoggerTests_StrangeOrder()
		{
			// m1 ends before m2 (can happen in multithreaded environment)
			var m1 = PerformanceLogger.Start();
			var m2 = PerformanceLogger.Start();
			m1.Dispose();
			m2.Dispose();
		}

		public void DoStuff()
		{
			using (var measurement = PerformanceLogger.Start())
			{
				measurement.SetMetadata("metadata_test", "true");

				DoSomeStuff();
			}
		}

		public void DoSomeStuff()
		{
			using (PerformanceLogger.Start())
			{
				// do nothing
			}
		}
	}
}