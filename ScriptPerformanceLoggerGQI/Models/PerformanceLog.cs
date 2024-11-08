﻿//------------------------------------------------------------------------------
// This code was copied from ScriptPerformanceLogger/Loggers/PerformanceFileLogger.
// Changes to this will result in errors.
//------------------------------------------------------------------------------
namespace Skyline.DataMiner.Utils.ScriptPerformanceLoggerGQI.Models
{
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;

    internal class PerformanceLog
    {
        [JsonProperty(Order = 0)]
        public string Name { get; set; }

        [JsonProperty(Order = 1)]
        public IReadOnlyDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        [JsonProperty(Order = 2)]
        public IReadOnlyList<PerformanceData> Data { get; set; } = new List<PerformanceData>();

        [JsonIgnore]
        public bool Any => (Metadata?.Any() == true) || (Data?.Any() == true);

        public bool ShouldSerializeMetadata()
        {
            return Metadata.Count > 0;
        }
    }
}