namespace Skyline.DataMiner.Utils.ScriptPerformanceLogger
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.CompilerServices;
	using System.Threading;

	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Interfaces;
	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Loggers;
	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Models;

	public class PerformanceMetrics : IDisposable
	{
		private static readonly ConcurrentDictionary<int, Stack<PerformanceData>> _threadMethodStacks = new ConcurrentDictionary<int, Stack<PerformanceData>>();

		private readonly bool _isMultiThreaded;
		private readonly PerformanceCollector _collector;
		private bool _disposed;
		private bool _isStarted = false;

		public PerformanceMetrics(bool startNow = true) : this()
		{
			_collector = new PerformanceCollector(new PerformanceLogger($"default-thread-{Thread.CurrentThread.ManagedThreadId}"));

			if (startNow)
				AutoStart();
		}

		public PerformanceMetrics(PerformanceCollector collector, bool startNow = true) : this()
		{
			_collector = collector ?? throw new ArgumentNullException(nameof(collector));

			if (startNow)
				AutoStart();
		}

		private PerformanceMetrics()
		{
			if (_threadMethodStacks.TryAdd(Thread.CurrentThread.ManagedThreadId, new Stack<PerformanceData>()))
				_isMultiThreaded = _threadMethodStacks.Count > 1;
		}

		public TimeSpan Elapsed => _collector.Elapsed;

		private Stack<PerformanceData> MethodsStack => _threadMethodStacks[Thread.CurrentThread.ManagedThreadId];

		[MethodImpl(MethodImplOptions.NoInlining)]
		private PerformanceData AutoStart()
		{
			MethodBase methodMemberInfo = new StackTrace().GetFrames().Where(frame => frame.GetMethod().Name != ".ctor").Skip(1).FirstOrDefault().GetMethod();
			string className = methodMemberInfo.DeclaringType.Name;
			string methodName = methodMemberInfo.Name;

			return Start(className, methodName);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public PerformanceData Start(int stackDepth = 1)
		{
			if (_isStarted)
				return MethodsStack.Peek();

			MethodBase methodMemberInfo = new StackTrace().GetFrame(stackDepth).GetMethod();
			string className = methodMemberInfo.DeclaringType.Name;
			string methodName = methodMemberInfo.Name;

			return Start(className, methodName);
		}

		public PerformanceData Start(string className, string methodName)
		{
			if (_isStarted)
				return MethodsStack.Peek();

			var methodData = new PerformanceData(className, methodName) { ThreadId = Thread.CurrentThread.ManagedThreadId };

			if (MethodsStack.Any())
				MethodsStack.Peek().SubMethods.Add(methodData);

			MethodsStack.Push(_collector.Start(methodData));

			_isStarted = true;

			return methodData;
		}

		private PerformanceData End()
		{
			if (MethodsStack.Any())
				return _collector.Stop(MethodsStack.Pop());

			return null;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed && disposing)
			{
				End();
				if (!MethodsStack.Any())
				{
					if (_isMultiThreaded)
						_threadMethodStacks.TryRemove(Thread.CurrentThread.ManagedThreadId, out _);

					_collector.Dispose();
				}
			}

			_disposed = true;
		}
	}
}