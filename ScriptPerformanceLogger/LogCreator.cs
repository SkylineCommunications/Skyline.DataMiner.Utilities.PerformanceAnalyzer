namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.CompilerServices;

	public class LogCreator
	{
		private MethodInvocation currentMethodInvocation;

		public Result Result { get; } = new Result();

		public IDisposable StartDisposableMethodCallMetric(string className, string methodName)
		{
			return new DisposableCapture(this, className, methodName);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public IDisposable StartDisposableMethodCallMetric()
		{
			MethodBase methodBase = new StackTrace().GetFrame(1).GetMethod();
			string className = methodBase.ReflectedType?.FullName;
			return new DisposableCapture(this, className, methodBase.Name);
		}

		private static List<MethodInvocation> FlattenMethodCallMetrics(IEnumerable<MethodInvocation> methodCallMetrics)
		{
			var flattenedMetrics = new List<MethodInvocation>();
			foreach (MethodInvocation metric in methodCallMetrics)
			{
				if (metric == null)
				{
					continue;
				}

				flattenedMetrics.Add(metric);
				flattenedMetrics.AddRange(FlattenMethodCallMetrics(metric.ChildInvocations));
			}

			return flattenedMetrics;
		}

		private void StartMethodCallMetric(string className, string methodName)
		{
			if (String.IsNullOrWhiteSpace(className))
			{
				throw new ArgumentNullException(nameof(className));
			}

			if (String.IsNullOrWhiteSpace(methodName))
			{
				throw new ArgumentNullException(nameof(methodName));
			}

			var newMethodCallMetric = new MethodInvocation
			{
				ClassName = className,
				MethodName = methodName,
				TimeStamp = DateTime.Now,
			};

			if (currentMethodInvocation == null)
			{
				Result.MethodInvocations.Add(newMethodCallMetric);
			}
			else
			{
				currentMethodInvocation.ChildInvocations.Add(newMethodCallMetric);
			}

			currentMethodInvocation = newMethodCallMetric;
		}

		private void CompleteMethodCallMetric(TimeSpan? executionTime)
		{
			currentMethodInvocation.ExecutionTime = executionTime ?? TimeSpan.Zero;
			currentMethodInvocation = GetParentMethodCallMetric(currentMethodInvocation);
		}

		private MethodInvocation GetParentMethodCallMetric(MethodInvocation childMethodInvocation)
		{
			var allMetrics = FlattenMethodCallMetrics(Result.MethodInvocations);

			return allMetrics.SingleOrDefault(m => m.ChildInvocations.Contains(childMethodInvocation));
		}

		private class DisposableCapture : IDisposable
		{
			private readonly LogCreator logCreator;
			private readonly Stopwatch stopwatch = new Stopwatch();

			public DisposableCapture(LogCreator logCreator, string className, string methodName)
			{
				this.logCreator = logCreator;
				logCreator.StartMethodCallMetric(className, methodName);
				stopwatch.Start();
			}

			public void Dispose()
			{
				stopwatch.Stop();
				logCreator.CompleteMethodCallMetric(stopwatch.Elapsed);
			}
		}
	}
}
