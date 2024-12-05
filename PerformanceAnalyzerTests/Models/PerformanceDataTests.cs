namespace Skyline.DataMiner.Utils.PerformanceAnalyzerTests.Models
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.Utils.PerformanceAnalyzer.Models;

	[TestClass]
	public class PerformanceDataTests
	{
		[TestMethod]
		public void PerformanceData_AddMetadata_ShouldAddMetadata()
		{
			// Arrange
			var data = new PerformanceData();

			// Act
			data.AddMetadata("key1", "value1")
				.AddMetadata("key2", "value2");

			// Assert
			Assert.AreEqual("value1", data.MetadataConcurrent["key1"]);
			Assert.AreEqual("value2", data.MetadataConcurrent["key2"]);
		}
	}
}