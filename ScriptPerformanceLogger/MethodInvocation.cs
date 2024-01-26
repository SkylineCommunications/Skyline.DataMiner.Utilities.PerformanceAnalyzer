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

		internal MethodInvocation(string className, string methodName, IDictionary<string, string> metadata = null)
		{
			ClassName = className ?? throw new ArgumentNullException(nameof(className));
			MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));

			if (metadata != null)
			{
				AddMetadata(metadata);
			}
		}

		public MethodInvocation(string className, string methodName, DateTime timeStamp, TimeSpan executionTime, IDictionary<string, string> metadata = null)
			: this(className, methodName, metadata)
		{
			SetStartTime(timeStamp);
			SetExecutionTime(executionTime);
		}

		[JsonProperty(Order = 0)]
		public string ClassName { get; private set; }

		[JsonProperty(Order = 1)]
		public string MethodName { get; private set; }

		[JsonProperty(Order = 2)]
		public DateTime TimeStamp { get; private set; }

		[JsonProperty(Order = 3)]
		public TimeSpan ExecutionTime { get; private set; }

		[JsonProperty(Order = 4)]
		public List<MethodInvocation> ChildInvocations { get; private set; } = new List<MethodInvocation>();

		[JsonProperty(Order = 5)]
		public Dictionary<string, string> Metadata { get; private set; } = new Dictionary<string, string>();

		// called by NewtonSoft, must be public
		public bool ShouldSerializeChildInvocations()
		{
			return ChildInvocations.Count > 0;
		}

		// called by NewtonSoft, must be public
		public bool ShouldSerializeMetadata()
		{
			return Metadata.Count > 0;
		}

		internal void SetStartTime(DateTime startTime)
		{
			TimeStamp = startTime;
		}

		internal void SetExecutionTime(TimeSpan executionTime)
		{
			ExecutionTime = executionTime;
		}

		internal void AddMetadata(IDictionary<string, string> metadata)
		{
			foreach (var item in metadata)
			{
				Metadata[item.Key] = item.Value;
			}
		}
	}
}
