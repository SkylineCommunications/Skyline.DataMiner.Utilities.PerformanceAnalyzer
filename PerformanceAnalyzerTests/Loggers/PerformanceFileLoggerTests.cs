namespace Skyline.DataMiner.Utils.PerformanceAnalyzerTests.Loggers
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.Utils.PerformanceAnalyzer.Loggers;
	using Skyline.DataMiner.Utils.PerformanceAnalyzer.Models;

	[TestClass]
	public class PerformanceFileLoggerTests
	{
		private string testDirectory;

		[TestInitialize]
		public void Setup()
		{
			// Set up a temporary directory for file tests
			testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(testDirectory);
		}

		[TestCleanup]
		public void Cleanup()
		{
			// Clean up temporary directory
			if (Directory.Exists(testDirectory))
			{
				Directory.Delete(testDirectory, true);
			}
		}

		[TestMethod]
		public void PerformanceFileLogger_AddMetadata_ShouldAddCorrectly()
		{
			// Arrange
			var logger = new PerformanceFileLogger("Collection1");

			// Act
			logger.AddMetadata("key1", "value1")
				  .AddMetadata("key2", "value2");

			// Assert
			Assert.AreEqual(2, logger.Metadata.Count);
			Assert.AreEqual("value1", logger.Metadata["key1"]);
			Assert.AreEqual("value2", logger.Metadata["key2"]);
		}

		[TestMethod]
		public void Constructor_WithFileNameOnly_ShouldUseDefaultDirectoryPath()
		{
			// Arrange
			var logger = new PerformanceFileLogger("Collection1", "test_log");

			// Act
			var logFileInfo = logger.LogFiles.First();

			// Assert
			Assert.AreEqual("test_log", logFileInfo.FileName);
			Assert.AreEqual(@"C:\Skyline_Data\PerformanceLogger", logFileInfo.FilePath);
		}

		[TestMethod]
		public void Constructor_WithFileNameAndFilePath_ShouldCreateLogFileInfo()
		{
			// Arrange
			var logger = new PerformanceFileLogger("Collection1", "test_log", @"C:\TestPath\TestLog");

			// Act
			var logFileInfo = logger.LogFiles.First();

			// Assert
			Assert.AreEqual("test_log", logFileInfo.FileName);
			Assert.AreEqual(@"C:\TestPath\TestLog", logFileInfo.FilePath);
		}

		[TestMethod]
		public void PerformanceFileLogger_AddMetadata_ShouldAddMetadataToDictionary()
		{
			// Arrange
			var logger = new PerformanceFileLogger("Collection1");
			var metadata = new Dictionary<string, string>
			{
				{ "key1", "value1" },
				{ "key2", "value2" },
			};

			// Act
			logger.AddMetadata(metadata);

			// Assert
			Assert.AreEqual(2, logger.Metadata.Count);
			Assert.AreEqual("value1", logger.Metadata["key1"]);
			Assert.AreEqual("value2", logger.Metadata["key2"]);
		}

		[TestMethod]
		public void PerformanceFileLogger_Report_CreatesFileWithCorrectData()
		{
			// Arrange
			var expectedFileContent = @"[{""Name"":""Collection1"",""StartTime"":""[STARTTIME]"",""Metadata"":{""key1"":""value1""},""Data"":[{""ClassName"":""Program"",""MethodName"":""Main"",""StartTime"":""2024-12-12T14:15:22Z"",""ExecutionTime"":""00:00:00.1000000""}]}]";
			var logFileInfo = new LogFileInfo("test_log", testDirectory);
			var logger = new PerformanceFileLogger("Collection1", logFileInfo);

			expectedFileContent = expectedFileContent.Replace("[STARTTIME]", logger.StartTime.ToString("O"));

			var performanceData = new List<PerformanceData>
			{
				new PerformanceData
				{
					ClassName = "Program",
					MethodName = "Main",
					StartTime = new DateTime(2024, 12, 12, 14, 15, 22, DateTimeKind.Utc),
					ExecutionTime = new TimeSpan(1_000_000),
				},
			};

			logger.AddMetadata("key1", "value1");

			// Act
			logger.Report(performanceData);

			// Assert
			string expectedFilePath = Path.Combine(testDirectory, "test_log.json");
			Assert.IsTrue(File.Exists(expectedFilePath));

			string fileContent = File.ReadAllText(expectedFilePath);
			Assert.AreEqual(expectedFileContent, fileContent);
		}

		[TestMethod]
		public void PerformanceFileLogger_Report_DoesNotContainMetadataFieldIfMetadataIsEmpty()
		{
			// Arrange
			var logFileInfo = new LogFileInfo("test_log", testDirectory);
			var logger = new PerformanceFileLogger("Collection1", logFileInfo);

			var performanceData = new List<PerformanceData>
			{
				new PerformanceData
				{
					ClassName = "Program",
					MethodName = "Main",
					StartTime = new DateTime(2024, 12, 12, 14, 15, 22, DateTimeKind.Utc),
					ExecutionTime = new TimeSpan(1_000_000),
				},
			};

			// Act
			logger.Report(performanceData);

			// Assert
			string expectedFilePath = Path.Combine(testDirectory, "test_log.json");
			Assert.IsTrue(File.Exists(expectedFilePath));

			string fileContent = File.ReadAllText(expectedFilePath);
			Assert.IsFalse(fileContent.Contains("Metadata"));
		}

		[TestMethod]
		public void PerformanceFileLogger_Report_AppendsDataIfFileAlreadyExists()
		{
			// Arrange
			string expectedFilePath = Path.Combine(testDirectory, "test_log.json");

			var existingDataInFile = @"[{""Name"":""Collection1"",""StartTime"":""2024-12-12T14:15:22Z"",""Metadata"":{""key1"":""value1""},""Data"":[{""ClassName"":""Program"",""MethodName"":""Main"",""StartTime"":""2024-12-12T14:15:22Z"",""ExecutionTime"":""00:00:00.1000000""}]}]";
			var expectedFileContent = @"[{""Name"":""Collection1"",""StartTime"":""2024-12-12T14:15:22Z"",""Metadata"":{""key1"":""value1""},""Data"":[{""ClassName"":""Program"",""MethodName"":""Main"",""StartTime"":""2024-12-12T14:15:22Z"",""ExecutionTime"":""00:00:00.1000000""}]},{""Name"":""Collection1"",""StartTime"":""[STARTTIME]"",""Metadata"":{""key2"":""value2""},""Data"":[{""ClassName"":""NotProgram"",""MethodName"":""Foo"",""StartTime"":""2023-11-10T09:08:07Z"",""ExecutionTime"":""00:00:00.2000000""}]}]";

			File.Create(expectedFilePath).Close();
			File.WriteAllText(expectedFilePath, existingDataInFile);

			var logFileInfo = new LogFileInfo("test_log", testDirectory);
			var logger = new PerformanceFileLogger("Collection1", logFileInfo);

			expectedFileContent = expectedFileContent.Replace("[STARTTIME]", logger.StartTime.ToString("O"));

			var performanceData = new List<PerformanceData>
			{
				new PerformanceData
				{
					ClassName = "NotProgram",
					MethodName = "Foo",
					StartTime = new DateTime(2023, 11, 10, 09, 08, 07, DateTimeKind.Utc),
					ExecutionTime = new TimeSpan(2_000_000),
				},
			};

			logger.AddMetadata("key2", "value2");

			// Act
			logger.Report(performanceData);

			// Assert
			Assert.IsTrue(File.Exists(expectedFilePath));

			string fileContent = File.ReadAllText(expectedFilePath);
			Assert.AreEqual(expectedFileContent, fileContent);
		}

		[TestMethod]
		public void PerformanceFileLogger_IncludeDateInFileName()
		{
			// Arrange
			var logger = new PerformanceFileLogger("Collection1", new LogFileInfo("test_log", testDirectory)) { IncludeDate = true };

			var performanceData = new List<PerformanceData>
			{
				new PerformanceData
				{
					ClassName = "Program",
					MethodName = "Main",
					StartTime = new DateTime(2024, 12, 12, 14, 15, 22, DateTimeKind.Utc),
					ExecutionTime = new TimeSpan(1_000_000),
				},
			};

			// Act
			logger.Report(performanceData);

			// Assert
			var files = Directory.GetFiles(testDirectory);
			Assert.AreEqual(1, files.Length);

			string fileName = Path.GetFileName(files.First());
			Assert.IsTrue(fileName.Contains("test_log"));
			Assert.IsTrue(fileName.Contains(DateTime.UtcNow.ToString("yyyy-MM-dd")));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void LogFileInfo_ThrowsArgumentException_WhenFileNameIsNull()
		{
			// Arrange
			string fileName = null;
			string filePath = "valid/path";

			// Act
			_ = new LogFileInfo(fileName, filePath);

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void LogFileInfo_ThrowsArgumentException_WhenFileNameIsWhitespace()
		{
			// Arrange
			string fileName = "   ";
			string filePath = "valid/path";

			// Act
			_ = new LogFileInfo(fileName, filePath);

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void LogFileInfo_ThrowsArgumentException_WhenFilePathIsNull()
		{
			// Arrange
			string fileName = "log.txt";
			string filePath = null;

			// Act
			_ = new LogFileInfo(fileName, filePath);

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void LogFileInfo_ThrowsArgumentException_WhenFilePathIsWhitespace()
		{
			// Arrange
			string fileName = "log.txt";
			string filePath = "   ";

			// Act
			_ = new LogFileInfo(fileName, filePath);

			// Assert is handled by ExpectedException
		}

		[TestMethod]
		public void PerformanceLog_Any_ReturnsTrueIfMetadataExists()
		{
			// Arrange & Act
			var log = new PerformanceLog()
			{
				Metadata = new Dictionary<string, string>() { { "key1", "value1" } },
			};

			// Assert
			Assert.IsTrue(log.Any);
		}

		[TestMethod]
		public void PerformanceLog_Any_ReturnsTrueIfDataExists()
		{
			// Arrange & Act
			var log = new PerformanceLog()
			{
				Data = new List<PerformanceData>() { new PerformanceData() },
			};

			// Assert
			Assert.IsTrue(log.Any);
		}

		[TestMethod]
		public void PerformanceLog_Any_ReturnsFalseIfDataAndMetadataAreEmpty()
		{
			// Arrange & Act
			var log = new PerformanceLog();

			// Assert
			Assert.IsFalse(log.Any);
		}

		[TestMethod]
		public void PerformanceLog_Any_ReturnsFalseIfDataAndMetadataAreNull()
		{
			// Arrange & Act
			var log = new PerformanceLog()
			{
				Metadata = null,
				Data = null,
			};

			// Assert
			Assert.IsFalse(log.Any);
		}
	}
}