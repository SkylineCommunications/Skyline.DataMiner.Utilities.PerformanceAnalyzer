namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;

	using Newtonsoft.Json;

	[Serializable]
	public class MethodInvocation
	{
		private readonly Stopwatch stopwatch;
		private MethodInvocation firstMethodInvocation;

		[JsonConstructor]
		private MethodInvocation()
		{
		}

		public MethodInvocation(string className, string methodName, LogCreator logCreator)
		{
			ClassName = className;
			MethodName = methodName;
			stopwatch = new Stopwatch();

			firstMethodInvocation = logCreator.Result.MethodInvocations.FirstOrDefault();
		}

		[JsonProperty(Order = 0)]
		public string ClassName { get; private set;}

		[JsonProperty(Order = 1)]
		public string MethodName { get; private set; }

		[JsonProperty(Order = 2)]
		public DateTime TimeStamp { get; private set; }

		[JsonProperty(Order = 3)]
		public TimeSpan ExecutionTime { get; private set; }

		[JsonProperty(Order = 4)]
		public List<MethodInvocation> ChildInvocations { get; private set; } = new List<MethodInvocation>();

		public void Start()
		{
			TimeStamp = firstMethodInvocation == null
				? DateTime.UtcNow
				: firstMethodInvocation.TimeStamp.AddTicks(this.firstMethodInvocation.stopwatch.Elapsed.Ticks);

			stopwatch.Start();
		}

		public void Stop()
		{
			ExecutionTime = stopwatch.Elapsed;
		}
	}
}
