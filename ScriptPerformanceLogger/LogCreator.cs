namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger
{
	using System;
	using System.Collections.Generic;

	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Tools;

	public class LogCreator
	{
		private readonly Stack<MethodInvocation> _runningMethods = new Stack<MethodInvocation>();
		private readonly HashSet<MethodInvocation> _endedMethods = new HashSet<MethodInvocation>();

		public Result Result { get; } = new Result();

		public HighResClock Clock { get; } = new HighResClock();

		public Measurement StartMeasurement(string className, string methodName)
		{
			lock (_runningMethods)
			{
				var invocation = new MethodInvocation(className, methodName);

				if (_runningMethods.Count > 0)
				{
					_runningMethods.Peek().ChildInvocations.Add(invocation);
				}
				else
				{
					Result.MethodInvocations.Add(invocation);
				}

				_runningMethods.Push(invocation);

				return new Measurement(this, invocation);
			}
		}

		internal void EndMeasurement(Measurement measurement)
		{
			lock (_runningMethods)
			{
				_endedMethods.Add(measurement.Invocation);

				while (_runningMethods.Count > 0 && _endedMethods.Contains(_runningMethods.Peek()))
				{
					var method = _runningMethods.Pop();
				}
			}
		}

		public void RegisterResult(MethodInvocation methodInvocation)
		{
			if (methodInvocation == null)
				throw new ArgumentNullException(nameof(methodInvocation));

			lock (_runningMethods)
			{
				if (_runningMethods.Count > 0)
				{
					_runningMethods.Peek().ChildInvocations.Add(methodInvocation);
				}
				else
				{
					Result.MethodInvocations.Add(methodInvocation);
				}
			}
		}
	}
}
