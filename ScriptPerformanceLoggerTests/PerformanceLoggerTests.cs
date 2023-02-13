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
		public void PerformanceLoggerTests()
		{
			// act
			PerformanceLogger.SetProperty("Script Name", "MyTestScript");
			PerformanceLogger.SetProperty("Start Time", DateTime.UtcNow.ToString("O"));
			DoStuff();
			Result result = PerformanceLogger.PerformCleanupAndReturn();

			// assert
			result.MethodInvocations.Should().HaveCount(1);
			result.MethodInvocations.Single().MethodName.Should().Be(nameof(DoStuff));
			result.MethodInvocations.Single().ChildInvocations.Single().MethodName.Should().Be(nameof(DoSomeStuff));
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