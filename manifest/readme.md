### Performance Analyzer

#### Overview

[Performance Analyzer NuGet]() allows us to gather the performance metrics for our solution, but collecting the data is only piece of the puzzle. If we want the collected metrics to bring value, we need to be able to interpret them and use them for decision-making. As humans, we often struggle to comprehend numbers, particularly when they are extremely large or, as is often the case with **performance metrics**, very small. This makes it essential to provide a more **intuitive way to interpret the data**, such as **visualization**. Performance Analyzer provides a **LCA that visualizes the metrics out of the box**, along with a **collection of GQIs that allow for custom visualizations**, tailored to the specific requirements of a project.

![Performance Analyzer LCA](./Images/lca_timeline.png)

> [!IMPORTANT]
> This package supplements the [Performance Analyzer NuGet](https://github.com/SkylineCommunications/Skyline.DataMiner.Utils.PerformanceAnalyzer). Implementing it in your code is a prerequisite.

> [!NOTE] 
> Detailed introduction to Performance Analyzer NuGet and the LCA can be found on our docs: [Introduction to Performance Analyzer]().

#### Use Cases

- **Monitoring**: Performance Analyzer enables **continuous monitoring of your solution’s performance**, providing real-time insights into critical metrics. By visualizing these metrics, you can quickly identify performance trends, detect potential issues before they impact the system, and **ensure the solution meets expected performance standards**. This ongoing oversight supports proactive management and optimization, helping to maintain system reliability and enhance user experience.
- **Debugging**: Performance Analyzer aids in **pinpointing performance-related issues** by providing detailed metric visualizations that highlight bottlenecks and inefficiencies in the system. Through targeted data analysis, it enables a deeper understanding of the underlying causes of performance drops, helping to **accelerate the debugging process**. By visualizing these metrics, you can more easily trace issues back to specific components, optimize resource allocation, and ensure smoother, more reliable performance in your solution.

> [!NOTE]
> [Performance Analyzer](https://github.com/SkylineCommunications/Skyline.DataMiner.Utils.PerformanceAnalyzer) library is open source project, pull requests are welcomed.

#### Prerequisites

To deploy this integration from the Catalog, you’ll need:

- **DataMiner** version 10.2.7 or higher.
- Implementation of **Performance Analyzer** in your code.

#### Installation and Configuration

##### Step 1: Implementing Performance Analyzer NuGet

1. Implement the [Performance Analyzer NuGet]() in your code.

> [!NOTE]
> Detailed documentation is available in [Performance Analyzer ReadMe](https://github.com/SkylineCommunications/Skyline.DataMiner.Utils.PerformanceAnalyzer/blob/3.0.X/README.md)

##### Step 2: Deploying Performance Analyzer LCA

1. Hit the **Deploy** button and deploy the package directly to your DataMiner system with a single click.
2. Optionally verify that the deployment was successful in [admin.dataminer.services](https://admin.dataminer.services/).

##### Step 3: Accessing Performance Analyzer LCA

1. Go to `http(s)://[DMA name]/root`.
1. Select *Performance Analyzer* to start using the application.

#### Using the application

To view your metrics, begin by specifying the path to your Performance Analyzer logs. Once set, you can visualize the execution of a specific method by selecting the relevant log file, locating the run where the execution took place, and double-clicking on the method you want to visualize.

![Performance Analyzer LCA Flow](./Images/lca_flow.gif)

#### Support

For additional help, reach out to support at [techsupport@skyline.be](mailto:techsupport@skyline.be)