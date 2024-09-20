namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger.Models
{
	using System;
	using System.Collections.Generic;

	using Newtonsoft.Json;

	[Serializable]
	public class PerformanceData
	{
		[JsonConstructor]
		public PerformanceData()
		{
		}

		public PerformanceData(string className, string methodName)
		{
			ClassName = className;
			MethodName = methodName;
		}

		public PerformanceData(PerformanceData methodData) : this(methodData.ClassName, methodData.MethodName)
		{
		}

		[JsonProperty(Order = 0)]
		public string ClassName { get; set; }

		[JsonProperty(Order = 1)]
		public string MethodName { get; set; }

		[JsonProperty(Order = 2)]
		public DateTime StartTime { get; set; }

		[JsonProperty(Order = 3)]
		public TimeSpan ExecutionTime { get; set; }

		[JsonProperty(Order = 4)]
		public int ThreadId { get; set; }

		[JsonProperty(Order = 5)]
		public List<PerformanceData> SubMethods { get; } = new List<PerformanceData>();

		[JsonProperty(Order = 6)]
		public Dictionary<string, string> Metadata { get; private set; } = new Dictionary<string, string>();

		public bool ShouldSerializeSubMethods()
		{
			return SubMethods.Count > 0;
		}

		public bool ShouldSerializeMetadata()
		{
			return Metadata.Count > 0;
		}
	}
}