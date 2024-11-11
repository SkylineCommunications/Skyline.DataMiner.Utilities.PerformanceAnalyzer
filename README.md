# About

The **Performance Analyzer** is a library designed to track and log performance metrics for methods in single or multi-threaded environments. It provides developers with an easy way to monitor execution times and track method calls across systems by logging performance data to the storage of their choice.

## Key Features

 - **Multi-threaded method tracking**: Handles performance tracking in single or multi-threaded environments.

 - **Method nesting**: Supports tracking of method calls within other methods (parent-child relationships).
 
 - **Performance data logging**: Logs performance metrics such as execution time, method metadata, and start/end times.
 
 - **Customizable logging**: Supports custom metadata and flexible file-based logging.
 
 - **JSON-based logs**: All logs are written in JSON format for easy parsing and analysis.

## Getting started

### TLDR

```PerformanceTracker``` will define the start and end of the method. Nesting ```PerformanceTracker``` instances will nest ```PerformanceData``` in the final JSON(*). When outer most ```PerformanceTracker```, on a specific thread, gets disposed of, it will call  ```PerformanceCollector.Dispose()``` for underlying collector.

When disposing of the ```PerformanceCollector``` we add the current threads methods performance metrics to the list of metrics to log. When the last thread, among threads that shared the same collector, disposes of the ```PerformanceCollector```, it will call ```IPerformanceLogger.Report(List<PerformanceData>)``` for the underlying implementation of the ```IPerformanceLogger```, that was passed in the constructor.

 > **_NOTE:_** (*) Nesting in multi-treaded use cases requires more setup, check [Examples with nesting (multi-threaded use case)](#examples-with-nesting-multi-threaded-use-case).

### Installation

To get started, simply add **Skyline.DataMiner.Utils.PerformanceAnalyzer** NuGet to your project.

### Types

The library exposes the following classes:
 - ```PerformanceTracker``` which is responsible for tracking method calls, method nesting in the logs and orchestration of collecting method performance metrics. It is the heart and soul of Performance Analyzer library.

 - ```PerformanceCollector``` which is responsible for collecting method performance metrics.

 - ```PerformanceFileLogger``` which is the implementation of IPerformanceLogger that logs to file system.

 - ```PerformanceData``` which is the model for method performance metrics.

 - ```LogFileInfo``` which represents information about a log file, including its name and path.

The library exposes the following interfaces:
 - ```IPerformanceLogger``` which contains only one method ```Report(List<PerformanceData>)```. This method will be called by ```PerformanceCollector``` when data is ready to be logged.

## Usage

```PerformanceTracker``` is the entry point for the library. It can be used to define start and end of the tracked method through ```using``` statement. Tracking will start when ```PerformanceTracker``` is initialized and end when it is disposed of, by calling ```Dispose```.

```PerformanceCollector``` collects method performance metrics to log. When ```PerformanceTracker``` is disposed of, it will pass the collected data to the underlying collector, once the collector is disposed of, it will call ```IPerformanceLogger.Report(List<PerformanceData>)``` which will handle logging of the method performance metrics.

```PerformanceFileLogger``` is default implementation of the ```IPerformanceLogger``` interface which logs to file(s). By default, the file location is *C:\Skyline_Data\PerformanceLogger* but this can be overwritten in the constructor. In cases where file(s) to log to don't already exist, they will be created, otherwise, new method performance metrics will be appended to the existing ones. ```PerformanceFileLogger``` defines metadata on the run level. It is also possible to include date and time in the file name by setting ```IncludeDate``` to ```true```.

```PerformanceData``` defines the format of the JSON which contains information about method performance metrics.

The following properties are defined for ```PerformanceData```
 - ```ClassName``` - name of the class in which the method is defined
 - ```MethodName``` - name of the method
 - ```StartTime``` - start time of the method execution
 - ```ExecutionTime``` - duration of the method execution
 - ```SubMethods``` - list of tracked methods that were invoked from this method
 - ```Metadata``` - dictionary that contains additional information about the method

 > **_NOTE:_** In case ```Metadata``` and/or ```SubMethods``` are empty they will not be included in the final JSON.

```IPerformanceLogger``` is the interface on which ```PerformanceCollector``` is based, implementing it provides alternative ways to log method performance metrics.

### Basic example

The most basic use case of the library. When dealing with single-threaded environments, the only responsibility of the developer is to provide the same collector to all instances of ```PerformanceTracker```. 

#### Input
```csharp
// Create a PerformanceFileLogger instance that will log to file 'dummy.json'
PerformanceFileLogger fileLogger = new PerformanceFileLogger("dummy");
// Create a PerformanceCollector instance with PerformanceFileLogger
PerformanceCollector collector = new PerformanceCollector(fileLogger);

void Foo()
{
  // Create a PerformanceTracker instance and start tracking.
  using (new PerformanceTracker(collector))
  {
    // Your code goes here...
    Bar();
    // Your code goes here...
  } // Tracker automatically adds performance data to the collector when disposed.
}

void Bar()
{
  using (new PerformanceTracker(collector))
  {
    // Your code goes here...
  }
}
```
The ```PerformanceTracker``` class exposes the underlying collector via ```Collector``` property, making it possible to provide collector directly from the instance of ```PerformanceTracker```.
```csharp
void Foo()
{
  // Create a PerformanceFileLogger instance that will log to file 'dummy.json'
  PerformanceFileLogger fileLogger = new PerformanceFileLogger("dummy");
  // Create a PerformanceCollector instance with PerformanceFileLogger
  PerformanceCollector collector = new PerformanceCollector(fileLogger);
  
  // Create a PerformanceTracker instance and start tracking.
  using (var tracker = new PerformanceTracker(collector))
  {
    // Your code goes here...
    Bar(tracker);
    // Your code goes here...
  } // Tracker automatically adds performance data to the collector when disposed.
}

void Bar(PerformanceTracker tracker)
{
  using (new PerformanceTracker(tracker.Collector))
  {
    // Your code goes here...
  }
}
```

#### Output
This will create a file *dummy.json* at *C:\Skyline_Data\PerformanceLogger* containing methods performance metrics.

The log file might look like this:
```json
[
  {
    "Data": [
      {
        "ClassName": "Program",
        "MethodName": "Foo",
        "StartTime": "2024-10-08T10:27:52.9476377Z",
        "ExecutionTime": "00:00:02.0185579",
        "SubMethods": [
          {
            "ClassName": "Program",
            "MethodName": "Bar",
            "StartTime": "2024-10-08T10:27:52.9478149Z",
            "ExecutionTime": "00:00:01.0077400"
          }
        ]
      }
    ]
  }
]
```

### Examples with metadata

It is possible to add metadata to tracked method's ```PerformanceData``` by calling ```PerformanceTracker.AddMetadata(string, string)```. Metadata can be used to include additional information for each method, for example thread id in multi-threaded environments. 

#### Input
```csharp
// Create a PerformanceFileLogger instance that will log to file 'dummy.json'
PerformanceFileLogger fileLogger = new PerformanceFileLogger("dummy");
// Create a PerformanceCollector instance with PerformanceFileLogger
PerformanceCollector collector = new PerformanceCollector(fileLogger);

void Foo()
{
  // Create a PerformanceTracker instance and start tracking.
  using (var tracker = new PerformanceTracker(collector))
  {
    tracker.AddMetadata("key", "value");
    // Your code goes here...
  } // Tracker automatically adds performance data to the collector when disposed.
}
```
or
```csharp
// Create a PerformanceFileLogger instance that will log to file 'dummy.json'
PerformanceFileLogger fileLogger = new PerformanceFileLogger("dummy");
// Create a PerformanceCollector instance with PerformanceFileLogger
PerformanceCollector collector = new PerformanceCollector(fileLogger);

void Foo()
{
  // Create a PerformanceTracker instance and start tracking.
  using (var tracker = new PerformanceTracker(collector).AddMetadata("key", "value"))
  {
    // Your code goes here...
  } // Tracker automatically adds performance data to the collector when disposed.
}
```

#### Output
This will create a file named *dummy.json* at *C:\Skyline_Data\PerformanceLogger* containing methods performance metrics and defined metadata for the containing method ```Foo()```.

The log file might look like this:
```json
[
  {
    "Data": [
      {
        "ClassName": "Program",
        "MethodName": "Foo",
        "StartTime": "2024-10-08T10:35:52.6521212Z",
        "ExecutionTime": "00:00:01.0088806",
        "Metadata": {
          "key": "value"
        }
      }
    ]
  }
]
```

### Examples with nesting (multi-threaded use case)

When it comes to multi-threaded use cases, we have a couple of options. 

 1. We can give each thread its own ```PerformanceCollector``` in which case the result will be one log file per thread.

#### Input
```csharp
// Create a PerformanceFileLogger instance that will log to file 'dummy.json'
PerformanceFileLogger fileLogger = new PerformanceFileLogger("dummyA");
// Create a PerformanceCollector instance with PerformanceFileLogger
PerformanceCollector collectorA = new PerformanceCollector(fileLogger);

void Foo()
{
  // Create a PerformanceTracker instance and start tracking.
  using (new PerformanceTracker(collectorA))
  {
    // Your code goes here...
    var task = Task.Run(() => Bar());
    // Your code goes here...
  } // Tracker automatically adds performance data to the collector when disposed.
}

void Bar()
{
  using (new PerformanceTracker(new PerformanceCollector(new PerformanceFileLogger("dummyB"))))
  {
    // Your code goes here...
  }
}
```
The log file *dummyA.json* might look like this:
```json
[
  {
    "Data": [
      {
        "ClassName": "Program",
        "MethodName": "Foo",
        "StartTime": "2024-10-08T11:27:16.6455682Z",
        "ExecutionTime": "00:00:00.6484858"
      }
    ]
  }
]
```
The log file *dummyB.json* might look like this:
```json
[
  {
    "Data": [
      {
        "ClassName": "Program",
        "MethodName": "Bar",
        "StartTime": "2024-10-08T11:27:16.6557048Z",
        "ExecutionTime": "00:00:00.5051130"
      }
    ]
  }
]
```
 2. We can share one ```PerformanceCollector``` instance between threads in which case we will end up with one log file, but without nesting.

```csharp
// Create a PerformanceFileLogger instance that will log to file 'dummy.json'
PerformanceFileLogger fileLogger = new PerformanceFileLogger("dummy");
// Create a PerformanceCollector instance with PerformanceFileLogger
PerformanceCollector collector = new PerformanceCollector(fileLogger);

void Foo()
{
  // Create a PerformanceTracker instance and start tracking.
  using (new PerformanceTracker(collector))
  {
    // Your code goes here...
    var task = Task.Run(() => Bar());
    // Your code goes here...
  } // Tracker automatically adds performance data to the collector when disposed.
}

void Bar()
{
  using (new PerformanceTracker(collector))
  {
    // Your code goes here...
  }
}
```
The log file might look like this:
```json
[
  {
    "Data": [
      {
        "ClassName": "Program",
        "MethodName": "Bar",
        "StartTime": "2024-10-08T11:13:18.5777648Z",
        "ExecutionTime": "00:00:00.5096074"
      },
      {
        "ClassName": "Program",
        "MethodName": "Foo",
        "StartTime": "2024-10-08T11:13:18.5678629Z",
        "ExecutionTime": "00:00:00.6250832"
      }
    ]
  }
]
```
> **_NOTE:_** Notice that order of entries is not the same as the order of methods.

 3. We can pass the ```PerformanceTracker``` instance, under which we want to nest methods performance metrics of the thread, in the constructor of the threads ```PerformanceTracker```. This is the way to achieve expected nesting when dealing with threads in most cases.

```csharp
// Create a PerformanceFileLogger instance that will log to file 'dummy.json'
PerformanceFileLogger fileLogger = new PerformanceFileLogger("dummy");
// Create a PerformanceCollector instance with PerformanceFileLogger
PerformanceCollector collector = new PerformanceCollector(fileLogger);

void Foo()
{
  using (var parentTracker = new PerformanceTracker(collector))
  {
    // Your code goes here...
    var task = Task.Run(() => Bar(parentTracker));
    // Your code goes here...
  } // Tracker automatically adds performance data to the collector when disposed.
}

void Bar(PerformanceTracker parentTracker)
{
  // Create a PerformanceTracker instance with the tracker under which to nest this method's performance metrics and start tracking.
  using (new PerformanceTracker(parentTracker))
  {
    // Your code goes here...
  }
}
```
The log file might look like this:
```json
[
  {
    "Data": [
      {
      "ClassName": "Program",
      "MethodName": "Foo",
      "StartTime": "2024-10-08T11:22:21.3431815Z",
      "ExecutionTime": "00:00:00.6309307",
      "SubMethods": [
          {
            "ClassName": "Program",
            "MethodName": "Bar",
            "StartTime": "2024-10-08T11:22:21.3549284Z",
            "ExecutionTime": "00:00:00.5130359"
          }
        ]
      }
    ]
  }
]
```

### Adding metadata on the run level

In some cases it is necessary to add additional information for each run. In those situation it is possible to add metadata on the run level by calling ```PerformanceFileLogger.AddMetadata(string, string)``` or ```PerformanceFileLogger.AddMetadata(IReadOnlyDictionary<string, string>)```.

#### Input
```csharp
// Create a PerformanceFileLogger instance that will log to file 'dummy.json'
PerformanceFileLogger fileLogger = new PerformanceFileLogger("dummy");
// Create a PerformanceCollector instance with PerformanceFileLogger
PerformanceCollector collector = new PerformanceCollector(fileLogger);

void Foo()
{
  fileLogger.AddMetadata("key1", "value1")
  // Create a PerformanceTracker instance and start tracking.
  using (new PerformanceTracker(collector))
  {
    // Your code goes here...
    Bar();
    // Your code goes here...
  } // Tracker automatically adds performance data to the collector when disposed.
}

void Bar()
{
  using (new PerformanceTracker(collector))
  {
    // Your code goes here...
  }
}
```

#### Output
This will create a file named *dummy.json* at *C:\Skyline_Data\PerformanceLogger* containing methods performance metrics and run metadata.

The log file might look like this:
```json
[
  {
    "Metadata": {
      "key1": "value1"
    },
    "Data": [
      {
        "ClassName": "Program",
        "MethodName": "Foo",
        "StartTime": "2024-10-17T05:48:38.5899954Z",
        "ExecutionTime": "00:00:00.7159599",
        "SubMethods": [
          {
            "ClassName": "Program",
            "MethodName": "Bar",
            "StartTime": "2024-10-17T05:48:38.5901597Z",
            "ExecutionTime": "00:00:00.4009077"
          }
        ]
      }
    ]
  }
]
```

### Manually providing class and method names

In the previous examples the names of the class and method that are seen in the logs are decided automatically base on where the ```PerformanceTracker``` is initialized, by default it will take the name of containing method and it's class. However, in certain cases this is not desired and for those situations it is possible to manually provide class name and method name for the resulting ```PerformanceData``` object. It is also possible to define different path for the log files by providing second argument to the constructor of ```PerformanceFileLogger```.

#### Input
```csharp
// Create a PerformanceFileLogger instance that will log to file 'dummy.json'
PerformanceFileLogger fileLogger = new PerformanceFileLogger("dummy", @"C:\Desktop\Logs");
// Create a PerformanceCollector instance with PerformanceFileLogger
PerformanceCollector collector = new PerformanceCollector(fileLogger);

void Foo()
{
  // Create a PerformanceTracker instance and start tracking.
  using (new PerformanceTracker(collector, "Random Class", "Random Method"))
  {
    // Your code goes here...
  } // Tracker automatically adds performance data to the collector when disposed.
}
```

#### Output
This will create a file named *dummy.json* at *C:\Desktop\Logs* containing methods performance metrics.

The log file might look like this:
```json
[
  {
    "Data": [
      {
        "ClassName": "Random Class",
        "MethodName": "Random Method",
        "StartTime": "2024-10-08T10:35:52.6521212Z",
        "ExecutionTime": "00:00:01.0088806",
      }
    ]
  }
]
```

> **_IMPORTANT:_** If you don't wait for all threads to finish before disposing of the outer most ```PerformanceTracker``` it is possible for the main thread to call ```PerformanceTracker.Dispose()``` for it before other threads have time to finish, in that case, the resulting data will be incomplete or even incorrect.

## About DataMiner

DataMiner is a transformational platform that provides vendor-independent control and monitoring of devices and services. Out of the box and by design, it addresses key challenges such as security, complexity, multi-cloud, and much more. It has a pronounced open architecture and powerful capabilities enabling users to evolve easily and continuously.

The foundation of DataMiner is its powerful and versatile data acquisition and control layer. With DataMiner, there are no restrictions to what data users can access. Data sources may reside on premises, in the cloud, or in a hybrid setup.

A unique catalog of 7000+ connectors already exists. In addition, you can leverage DataMiner Development Packages to build your own connectors (also known as "protocols" or "drivers").

> **Note**
> See also: [About DataMiner](https://aka.dataminer.services/about-dataminer).

## About Skyline Communications

At Skyline Communications, we deal with world-class solutions that are deployed by leading companies around the globe. Check out [our proven track record](https://aka.dataminer.services/about-skyline) and see how we make our customers' lives easier by empowering them to take their operations to the next level.