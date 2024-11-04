namespace PerformanceLoggerCleanupScript.Tests
{
	using System;
	using System.IO;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Moq;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.ScriptPerformanceLoggerCleanup;

	[TestClass]
	public class PerformanceCleanupScriptTests
	{
		private Mock<IEngine> _mockEngine;
		private Script _script;
		private string _testDirectory;

		[TestInitialize]
		public void Setup()
		{
			_mockEngine = new Mock<IEngine>();
			_script = new Script();

			// Set up a temporary directory for file tests
			_testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(_testDirectory);
		}

		[TestCleanup]
		public void Cleanup()
		{
			// Clean up temporary directory
			if (Directory.Exists(_testDirectory))
			{
				Directory.Delete(_testDirectory, true);
			}
		}

		[TestMethod]
		public void PerformanceCleanupScriptTests_Run_DirectoryNotFound_ExitsWithMessage()
		{
			// Arrange
			var mockDaysParam = new Mock<ScriptParam>();
			_mockEngine.Setup(e => e.GetScriptParam("Days of oldest performance info")).Returns(mockDaysParam.Object);
			mockDaysParam.Setup(sp => sp.Value).Returns("7");

			string nonExistingFolderPath = "C:\\Skyline_Data\\NonExistingFolder";
			var mockFolderParam = new Mock<ScriptParam>();
			_mockEngine.Setup(e => e.GetScriptParam("Folder path to performance info")).Returns(mockFolderParam.Object);
			mockFolderParam.Setup(sp => sp.Value).Returns(nonExistingFolderPath);

			if (Directory.Exists(nonExistingFolderPath))
			{
				Directory.Delete(nonExistingFolderPath, true);
			}

			// Act
			_script.Run(_mockEngine.Object);

			// Assert
			_mockEngine.Verify(e => e.ExitFail(It.Is<string>(s => s.Contains("Directory not found"))), Times.Once);
		}

		[TestMethod]
		public void PerformanceCleanupScriptTests_Run_ValidParameters_DeletesOldFiles()
		{
			// Arrange
			var mockDaysParam = new Mock<ScriptParam>();
			_mockEngine.Setup(e => e.GetScriptParam("Days of oldest performance info")).Returns(mockDaysParam.Object);
			mockDaysParam.Setup(sp => sp.Value).Returns("7");

			var mockFolderParam = new Mock<ScriptParam>();
			_mockEngine.Setup(e => e.GetScriptParam("Folder path to performance info")).Returns(mockFolderParam.Object);
			mockFolderParam.Setup(sp => sp.Value).Returns(_testDirectory);

			File.WriteAllText(Path.Combine(_testDirectory, "oldFile.txt"), "test content");
			File.SetLastWriteTime(Path.Combine(_testDirectory, "oldFile.txt"), DateTime.Now.AddDays(-10));

			File.WriteAllText(Path.Combine(_testDirectory, "newFile.txt"), "test content");

			// Act
			_script.Run(_mockEngine.Object);

			// Assert
			Assert.IsFalse(File.Exists(Path.Combine(_testDirectory, "oldFile.txt")), "Old file should be deleted.");
			Assert.IsTrue(File.Exists(Path.Combine(_testDirectory, "newFile.txt")), "New file should still exist.");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void PerformanceCleanupScriptTests_Initialize_InvalidDaysParameter_ThrowsArgumentException()
		{
			// Arrange
			var mockScriptParam = new Mock<ScriptParam>();
			_mockEngine.Setup(e => e.GetScriptParam("Days of oldest performance info")).Returns(mockScriptParam.Object);
			mockScriptParam.Setup(sp => sp.Value).Returns("invalid");

			// Act
			_script.Initialize(_mockEngine.Object);

			// Assert is handled by ExpectedException
		}
	}
}