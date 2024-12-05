namespace Skyline.DataMiner.Utils.PerformanceAnalyzer.Models
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;

	using Newtonsoft.Json;

	/// <summary>
	/// <see cref="PerformanceData"/> is a model for method performance metrics.
	/// </summary>
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

		[JsonIgnore]
		public bool IsStarted { get; internal set; }

		[JsonIgnore]
		public bool IsStopped { get; internal set; }

		[JsonIgnore]
		public PerformanceData Parent { get; set; }

		[JsonProperty(Order = 0)]
		public string ClassName { get; set; }

		[JsonProperty(Order = 1)]
		public string MethodName { get; set; }

		[JsonProperty(Order = 2)]
		public DateTime StartTime { get; set; }

		[JsonProperty(Order = 3)]
		public TimeSpan ExecutionTime { get; set; }

		[JsonProperty(Order = 4)]
		public List<PerformanceData> SubMethods => SubMethodsConcurrent.ToList();

		[JsonIgnore]
		public ConcurrentQueue<PerformanceData> SubMethodsConcurrent { get; } = new ConcurrentQueue<PerformanceData>();

		[JsonProperty(Order = 5)]
		public Dictionary<string, string> Metadata => MetadataConcurrent.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

		[JsonIgnore]
		public ConcurrentDictionary<string, string> MetadataConcurrent { get; private set; } = new ConcurrentDictionary<string, string>();

		public PerformanceData AddMetadata(string key, string value)
		{
			MetadataConcurrent[key] = value;
			return this;
		}

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