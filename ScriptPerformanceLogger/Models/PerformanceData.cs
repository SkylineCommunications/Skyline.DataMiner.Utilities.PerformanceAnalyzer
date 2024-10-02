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

		[JsonIgnore]
		public PerformanceData Parent { get; set; }

		[JsonProperty]
		public string ClassName { get; set; }

		[JsonProperty]
		public string MethodName { get; set; }

		[JsonProperty]
		public DateTime StartTime { get; set; }

		[JsonProperty]
		public TimeSpan ExecutionTime { get; set; }

		[JsonProperty]
		public List<PerformanceData> SubMethods { get; } = new List<PerformanceData>();

		[JsonProperty]
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