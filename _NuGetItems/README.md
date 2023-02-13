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