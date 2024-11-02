namespace ScriptPerformanceLoggerGQI
{
	using System.Collections.Generic;
	using System.IO;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Utils.ScriptPerformanceLoggerGQI.Models;

	[GQIMetaData(Name = "Get Performance Metrics")]
	public class GetPerformanceMetrics : IGQIDataSource, IGQIInputArguments
    {
        private readonly GQIStringArgument _folderPathArgument = new GQIStringArgument("Folder Path") { IsRequired = true };
        private readonly GQIStringArgument _fileNameArgument = new GQIStringArgument("File Name") { IsRequired = true };
        private List<PerformanceLog> _performanceMetrics;

        public GQIArgument[] GetInputArguments()
        {
            return new GQIArgument[] { _folderPathArgument, _fileNameArgument };
        }

        public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
        {
            var folderPath = args.GetArgumentValue(_folderPathArgument);
            var fileName = args.GetArgumentValue(_fileNameArgument);

            var rawJson = File.ReadAllText(Path.Combine(folderPath, fileName));

            _performanceMetrics = JsonConvert.DeserializeObject<List<PerformanceLog>>(rawJson);

            return default;
        }

        public GQIColumn[] GetColumns()
        {
            return new GQIColumn[5]
            {
                new GQIStringColumn("Class"),
                new GQIStringColumn("Method"),
                new GQIStringColumn("Start Time"),
                new GQIStringColumn("End Time"),
                new GQIIntColumn("Execution Time"),
            };
        }

        public GQIPage GetNextPage(GetNextPageInputArgs args)
        {
            var rows = new List<GQIRow>();

            foreach (var performanceMetric in _performanceMetrics)
            {
                foreach (var performanceData in performanceMetric.Data)
                {
                    ProcessSubMethods(performanceData, rows);
				}
            }

            return new GQIPage(rows.ToArray());
        }

        private void CreateRow(PerformanceData performanceData, List<GQIRow> rows)
		{
			rows.Add(new GQIRow(
		        new GQICell[]
		        {
                    new GQICell()
                    {
                        Value = performanceData.ClassName,
                    },
                    new GQICell()
                    {
                        Value = performanceData.MethodName,
                    },
                    new GQICell()
                    {
                        Value = performanceData.StartTime.ToString("dd/MM/yyyy HH:mm:ss.fff"),
                    },
                    new GQICell()
                    {
                        Value = (performanceData.StartTime + performanceData.ExecutionTime).ToString("dd/MM/yyyy HH:mm:ss.fff"),
                    },
                    new GQICell()
                    {
                        Value = (int)performanceData.ExecutionTime.TotalMilliseconds,
                    },
		        }));
		}

        private void ProcessSubMethods(PerformanceData data, List<GQIRow> rows)
		{
            if (data == null)
            {
                return;
            }

            CreateRow(data, rows);

            if (data.SubMethods != null && data.SubMethods.Count > 0)
			{
				foreach (var subMethod in data.SubMethods)
				{
					ProcessSubMethods(subMethod, rows);
				}
			}
		}
	}
}