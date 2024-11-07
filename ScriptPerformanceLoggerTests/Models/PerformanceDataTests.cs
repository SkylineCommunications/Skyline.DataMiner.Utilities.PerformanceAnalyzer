namespace ScriptPerformanceLoggerTests.Models
{
	using System;
	using System.Linq;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Models;

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
			Assert.AreEqual("value1", data.Metadata["key1"]);
			Assert.AreEqual("value2", data.Metadata["key2"]);
		}

		[TestMethod]
		public void PerformanceData_CompareCopies()
		{
			// Arrange
			var performanceDataNuGet = new Skyline.DataMiner.Utils.ScriptPerformanceLogger.Models.PerformanceData();
			var performanceDataGQI = new Skyline.DataMiner.Utils.ScriptPerformanceLoggerGQI.Models.PerformanceData();

			// Act
			var propertiesNuGet = performanceDataNuGet.GetType().GetProperties();
			var propertiesGQI = performanceDataGQI.GetType().GetProperties().Where(p => !string.Equals(p.Name, "ID", StringComparison.OrdinalIgnoreCase)).ToArray();

			bool isCopied = true;
			for (int i = 0; i < propertiesNuGet.Length; i++)
			{
				if (propertiesNuGet[i].Name == "SubMethods" || propertiesNuGet[i].Name == "Parent")
				{
					if (propertiesNuGet[i].Name != propertiesGQI[i].Name)
					{
						isCopied = false;
						break;
					}

					continue;
				}

				if (propertiesNuGet[i].Name != propertiesGQI[i].Name || propertiesNuGet[i].PropertyType != propertiesGQI[i].PropertyType)
				{
					isCopied = false;
					break;
				}
			}

			// Assert
			Assert.IsTrue(isCopied);
		}
	}
}