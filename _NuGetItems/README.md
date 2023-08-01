Simple utility to gather some performance metrics

Example:
```C#
public void Run(Engine engine)
{
    try
    {
        MyMethod(engine);
    }
    finally
    {
        PerformanceLogger.PerformCleanUpAndStoreResult("MyScript");
    }
}

public void MyMethod()
{
    using (PerformanceLogger.Start())
    {
        ...
    }
}
```

Custom metrics can also be added to the result:
```C#
public void Run(Engine engine)
{
    try
    {
		PerformanceLogger.RegisterResult("MyClass", "MyMethod1", DateTime.UtcNow, TimeSpan.FromMilliseconds(30));
		PerformanceLogger.RegisterResult("MyClass", "MyMethod2", DateTime.UtcNow, TimeSpan.FromMilliseconds(30));
    }
    finally
    {
        PerformanceLogger.PerformCleanUpAndStoreResult("MyScript");
    }
}
```
