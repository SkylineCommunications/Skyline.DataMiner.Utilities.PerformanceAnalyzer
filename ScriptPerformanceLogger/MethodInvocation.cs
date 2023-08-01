namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger
{
	using System;
	using System.Collections.Generic;

	using Newtonsoft.Json;

	[Serializable]
	public class MethodInvocation
	{
		[JsonConstructor]
		private MethodInvocation()
		{
		}

		internal MethodInvocation(string className, string methodName)
		{
			ClassName = className ?? throw new ArgumentNullException(nameof(className));
			MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
		}

		public MethodInvocation(string className, string methodName, DateTime timeStamp, TimeSpan executionTime)
		{
			ClassName = className ?? throw new ArgumentNullException(nameof(className));
			MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
			TimeStamp = timeStamp;
			ExecutionTime = executionTime;
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

		internal void SetExecutionTime(DateTime timeStamp, TimeSpan executionTime)
		{
			TimeStamp = timeStamp;
			ExecutionTime = executionTime;
		}
	}
}
