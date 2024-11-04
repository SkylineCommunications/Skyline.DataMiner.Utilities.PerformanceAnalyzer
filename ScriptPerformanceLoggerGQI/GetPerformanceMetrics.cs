namespace ScriptPerformanceLoggerGQI
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Utils.ScriptPerformanceLoggerGQI.Models;

	[GQIMetaData(Name = "Get Performance Metrics")]
	public class GetPerformanceMetrics : IGQIDataSource, IGQIInputArguments
	{
		private readonly GQIStringArgument _folderPathArgument = new GQIStringArgument("Folder Path") { IsRequired = false };
		private readonly GQIStringArgument _fileNameArgument = new GQIStringArgument("File Name") { IsRequired = true };
		private List<PerformanceLog> _performanceMetrics;

		public GQIArgument[] GetInputArguments()
		{
			return new GQIArgument[] { _folderPathArgument, _fileNameArgument };
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			var folderPath = String.IsNullOrWhiteSpace(args.GetArgumentValue(_folderPathArgument)) ? @"C:\Skyline_Data\PerformanceLogger" : args.GetArgumentValue(_folderPathArgument);
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
				new GQIDateTimeColumn("Start Time"),
				new GQIDateTimeColumn("End Time"),
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

		private void ProcessSubMethods(PerformanceData data, List<GQIRow> rows)
		{
			if (data == null)
			{
				return;
			}

			CreateRow(data, rows);

			if (data.SubMethods != null && data.SubMethods.Any())
			{
				foreach (var subMethod in data.SubMethods)
				{
					ProcessSubMethods(subMethod, rows);
				}
			}
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
						Value = performanceData.StartTime,
					},
					new GQICell()
					{
						Value = performanceData.StartTime + performanceData.ExecutionTime,
					},
					new GQICell()
					{
						Value = (int)performanceData.ExecutionTime.TotalMilliseconds,
					},
				}));
		}
	}
}