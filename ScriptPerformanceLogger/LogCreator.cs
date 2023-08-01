namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger
{
	using System;
	using System.Collections.Generic;

	public class LogCreator
	{
		private readonly Stack<MethodInvocation> _runningMethods = new Stack<MethodInvocation>();

		public Result Result { get; } = new Result();

		public void RegisterResult(MethodInvocation methodInvocation)
		{
			if (methodInvocation == null) throw new ArgumentNullException(nameof(methodInvocation));

			Result.MethodInvocations.Add(methodInvocation);
		}

		public Measurement StartMeasurement(string className, string methodName)
		{
			var invocation = StartMethodCallMetric(className, methodName);
			var measurement = new Measurement(this, invocation);

			return measurement;
		}

		private MethodInvocation StartMethodCallMetric(string className, string methodName)
		{
			var invocation = new MethodInvocation(className, methodName);

			if (_runningMethods.Count > 0)
			{
				_runningMethods.Peek().ChildInvocations.Add(invocation);
			}

			_runningMethods.Push(invocation);

			return invocation;
		}

		internal void CompleteMethodCallMetric(Measurement measurement)
		{
			var runningMethodInvocation = _runningMethods.Pop();

			if (runningMethodInvocation != measurement.Invocation)
			{
				throw new InvalidOperationException("Result of incorrect invocation received!");
			}

			runningMethodInvocation.SetExecutionTime(measurement.StartTime, measurement.Elapsed);

			if (_runningMethods.Count == 0)
			{
				RegisterResult(runningMethodInvocation);
			}
		}
	}
}
