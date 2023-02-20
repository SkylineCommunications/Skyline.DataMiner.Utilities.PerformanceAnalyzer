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
		private MethodInvocation runningMethodInvocation;

		public Result Result { get; } = new Result();

		public IDisposable StartDisposableMethodCallMetric(string className, string methodName)
		{
			var disposable = new DisposableCapture(this);
			StartMethodCallMetric(className, methodName);
			return disposable;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public IDisposable StartDisposableMethodCallMetric()
		{
			MethodBase methodBase = new StackTrace().GetFrame(1).GetMethod();
			string className = methodBase.ReflectedType?.FullName;
			var disposable = new DisposableCapture(this);
			StartMethodCallMetric(className, methodBase.Name);
			return disposable;
		}

		private static IEnumerable<MethodInvocation> GetInvocationsRecursive(IEnumerable<MethodInvocation> methodInvocations)
		{
			foreach (MethodInvocation invocation in methodInvocations)
			{
				if (invocation == null)
				{
					continue;
				}

				yield return invocation;

				foreach (MethodInvocation methodInvocation in GetInvocationsRecursive(invocation.ChildInvocations))
				{
					yield return methodInvocation;
				}
			}
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

			var methodInvocation = new MethodInvocation(className, methodName, this);

			if (runningMethodInvocation == null)
			{
				Result.MethodInvocations.Add(methodInvocation);
			}
			else
			{
				runningMethodInvocation.ChildInvocations.Add(methodInvocation);
			}

			runningMethodInvocation = methodInvocation;
			methodInvocation.Start();
		}

		private void CompleteMethodCallMetric()
		{
			runningMethodInvocation.Stop();
			runningMethodInvocation = GetParentMethodInvocation(runningMethodInvocation);
		}

		private MethodInvocation GetParentMethodInvocation(MethodInvocation methodInvocation)
		{
			// todo could be optimised
			IEnumerable<MethodInvocation> invocations = GetInvocationsRecursive(Result.MethodInvocations);
			return invocations.SingleOrDefault(m => m.ChildInvocations.Contains(methodInvocation));
		}

		private class DisposableCapture : IDisposable
		{
			private readonly LogCreator logCreator;

			public DisposableCapture(LogCreator logCreator)
			{
				this.logCreator = logCreator;
			}

			public void Dispose()
			{
				logCreator.CompleteMethodCallMetric();
			}
		}
	}
}
