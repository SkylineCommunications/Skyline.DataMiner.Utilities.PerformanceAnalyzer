namespace ScriptPerformanceLoggerTests
{
	using System;
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
			result.MethodInvocations.Single().MethodName.Should().Be(nameof(DoStuff));
			result.MethodInvocations.Single().ChildInvocations.Single().MethodName.Should().Be(nameof(DoSomeStuff));
		}

		[TestMethod]
		public void PerformanceLoggerTests_RegisterResult()
		{
			// act
			PerformanceLogger.SetProperty("Script Name", "MyTestScript");
			PerformanceLogger.SetProperty("Start Time", DateTime.UtcNow.ToString("O"));

			PerformanceLogger.RegisterResult("ScriptPerformanceLoggerTests.MetricCreatorTests", "DoStuff", DateTime.UtcNow, TimeSpan.FromMilliseconds(30));
			PerformanceLogger.RegisterResult("ScriptPerformanceLoggerTests.MetricCreatorTests", "DoSomeStuff", DateTime.UtcNow, TimeSpan.FromMilliseconds(30));

			var result = PerformanceLogger.PerformCleanupAndReturn();

			// assert
			result.MethodInvocations.Should().HaveCount(2);
			result.MethodInvocations[0].MethodName.Should().Be(nameof(DoStuff));
			result.MethodInvocations[1].MethodName.Should().Be(nameof(DoSomeStuff));
		}

		public void DoStuff()
		{
			using (PerformanceLogger.Start())
			{
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