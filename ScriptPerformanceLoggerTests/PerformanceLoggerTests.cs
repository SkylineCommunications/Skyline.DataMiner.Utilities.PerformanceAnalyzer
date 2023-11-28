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
			var logger = new PerformanceLogger();

			// act
			logger.SetProperty("Script Name", "MyTestScript");
			logger.SetProperty("Start Time", DateTime.UtcNow.ToString("O"));

			DoStuff(logger);

			var result = logger.PerformCleanupAndReturn();

			// assert
			result.MethodInvocations.Should().HaveCount(1);
			result.MethodInvocations.Single().MethodName.Should().Be(nameof(DoStuff));
			result.MethodInvocations.Single().ChildInvocations.Single().MethodName.Should().Be(nameof(DoSomeStuff));
		}

		[TestMethod]
		public void PerformanceLoggerTests_RegisterResult()
		{
			var logger = new PerformanceLogger();

			// act
			logger.SetProperty("Script Name", "MyTestScript");
			logger.SetProperty("Start Time", DateTime.UtcNow.ToString("O"));

			logger.RegisterResult("ScriptPerformanceLoggerTests.MetricCreatorTests", "DoStuff", DateTime.UtcNow, TimeSpan.FromMilliseconds(30));
			logger.RegisterResult("ScriptPerformanceLoggerTests.MetricCreatorTests", "DoSomeStuff", DateTime.UtcNow, TimeSpan.FromMilliseconds(30));

			var result = logger.PerformCleanupAndReturn();

			// assert
			result.MethodInvocations.Should().HaveCount(2);
			result.MethodInvocations[0].MethodName.Should().Be(nameof(DoStuff));
			result.MethodInvocations[1].MethodName.Should().Be(nameof(DoSomeStuff));
		}

		public void DoStuff(PerformanceLogger logger)
		{
			using (logger.StartMeasurement())
			{
				DoSomeStuff(logger);
			}
		}

		public void DoSomeStuff(PerformanceLogger logger)
		{
			using (logger.StartMeasurement())
			{
				// do nothing
			}
		}
	}
}