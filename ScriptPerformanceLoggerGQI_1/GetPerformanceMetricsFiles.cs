namespace Skyline.DataMiner.Utils.ScriptPerformanceLoggerGQI
{
	using System.Collections.Generic;
	using System.IO;

	using Skyline.DataMiner.Analytics.GenericInterface;
	using Skyline.DataMiner.Utils.ScriptPerformanceLoggerGQI.Models;

	[GQIMetaData(Name = "Get Performance Metrics Files")]
	public class GetPerformanceMetricsFiles : IGQIDataSource, IGQIInputArguments
	{
		private readonly GQIStringArgument _folderPathArgument = new GQIStringArgument("Folder Path") { IsRequired = true };
		private readonly List<FileMetadata> _filesMetadata = new List<FileMetadata>();

		public GQIArgument[] GetInputArguments()
		{
			return new GQIArgument[] { _folderPathArgument };
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			var folderPath = args.GetArgumentValue(_folderPathArgument);

			DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
			FileInfo[] files = directoryInfo.GetFiles();

			foreach (var file in files)
			{
				_filesMetadata.Add(new FileMetadata(file.Name, file.CreationTimeUtc, file.LastWriteTimeUtc, file.Length));
			}

			return default;
		}

		public GQIColumn[] GetColumns()
		{
			return new GQIColumn[4]
			{
				new GQIStringColumn("Name"),
				new GQIDateTimeColumn("Created"),
				new GQIDateTimeColumn("Last Modified"),
				new GQIDoubleColumn("Size"),
			};
		}

		public GQIPage GetNextPage(GetNextPageInputArgs args)
		{
			var rows = new List<GQIRow>();

			foreach (var fileMetadata in _filesMetadata)
			{
				rows.Add(new GQIRow(
						new GQICell[]
						{
							new GQICell()
							{
								Value = fileMetadata.Name,
							},
							new GQICell()
							{
								Value = fileMetadata.Created,
							},
							new GQICell()
							{
								Value = fileMetadata.LastModified,
							},
							new GQICell()
							{
								Value = (double)fileMetadata.Size,
							},
						}));
			}

			return new GQIPage(rows.ToArray());
		}
	}
}