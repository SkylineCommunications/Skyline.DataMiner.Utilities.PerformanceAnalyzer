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

	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Loggers;
	using Skyline.DataMiner.Utils.ScriptPerformanceLogger.Models;

	public sealed class PerformanceAnalyzer : IDisposable
	{
		private static readonly ConcurrentDictionary<int, Stack<PerformanceData>> _threadMethodStacks = new ConcurrentDictionary<int, Stack<PerformanceData>>();

		private readonly bool _isMultiThreaded;
		private readonly PerformanceCollector _collector;
		private readonly int _threadId = Thread.CurrentThread.ManagedThreadId;
		private PerformanceData _rootMethod;
		private bool _disposed;
		private bool _isStarted;

		public PerformanceAnalyzer(bool startNow = true) : this()
		{
			_collector = new PerformanceCollector(new PerformanceLogger($"default-thread-{Thread.CurrentThread.ManagedThreadId}"));

			if (startNow)
				AutoStart();
		}

		public PerformanceAnalyzer(PerformanceCollector collector, bool startNow = true) : this()
		{
			_collector = collector ?? throw new ArgumentNullException(nameof(collector));

			if (startNow)
				AutoStart();
		}

		public PerformanceAnalyzer(PerformanceAnalyzer parentPerformanceAnalyzer, bool startNow = true) : this()
		{
			_collector = parentPerformanceAnalyzer?.Collector ?? throw new ArgumentNullException(nameof(parentPerformanceAnalyzer));
			PerformanceData methodData;

			if (startNow)
			{
				methodData = AutoStart(parentPerformanceAnalyzer._threadId);
				parentPerformanceAnalyzer._rootMethod.SubMethods.Add(methodData);
				methodData.Parent = parentPerformanceAnalyzer._rootMethod;
			}
		}

		private PerformanceAnalyzer()
		{
			if (_threadMethodStacks.TryAdd(Thread.CurrentThread.ManagedThreadId, new Stack<PerformanceData>()))
				_isMultiThreaded = _threadMethodStacks.Count > 1;
		}

		public PerformanceCollector Collector => _collector ?? throw new InvalidOperationException(nameof(_collector));

		public PerformanceData RootMethod => _rootMethod ?? throw new InvalidOperationException(nameof(_rootMethod));

		public TimeSpan Elapsed => _collector?.Elapsed ?? throw new InvalidOperationException(nameof(_collector));

		private Stack<PerformanceData> MethodsStack => _threadMethodStacks[Thread.CurrentThread.ManagedThreadId];

		[MethodImpl(MethodImplOptions.NoInlining)]
		public PerformanceData Start(int stackDepth = 2)
		{
			return Start(Thread.CurrentThread.ManagedThreadId, stackDepth);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public PerformanceData Start(int threadId, int stackDepth = 1)
		{
			if (_isStarted)
				return MethodsStack.Peek();

			MethodBase methodMemberInfo = new StackTrace().GetFrame(stackDepth).GetMethod();
			string className = methodMemberInfo.DeclaringType.Name;
			string methodName = methodMemberInfo.Name;

			return Start(className, methodName, threadId);
		}

		public PerformanceData Start(string className, string methodName)
		{
			return Start(className, methodName, Thread.CurrentThread.ManagedThreadId);
		}

		public PerformanceData Start(string className, string methodName, int threadId)
		{
			if (_isStarted)
				return MethodsStack.Peek();

			var methodData = new PerformanceData(className, methodName);

			if (MethodsStack.Any())
			{
				MethodsStack.Peek().SubMethods.Add(methodData);
				methodData.Parent = MethodsStack.Peek();
			}

			MethodsStack.Push(_collector.Start(methodData, threadId));

			_rootMethod = methodData;
			_isStarted = true;

			return methodData;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private PerformanceData AutoStart()
		{
			MethodBase methodMemberInfo = new StackTrace().GetFrames().Where(frame => frame.GetMethod().Name != ".ctor")?.Skip(1)?.FirstOrDefault()?.GetMethod() ?? throw new InvalidOperationException(nameof(AutoStart));
			string className = methodMemberInfo.DeclaringType.Name;
			string methodName = methodMemberInfo.Name;

			return Start(className, methodName);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private PerformanceData AutoStart(int threadId)
		{
			MethodBase methodMemberInfo = new StackTrace().GetFrames().Where(frame => frame.GetMethod().Name != ".ctor")?.Skip(1)?.FirstOrDefault()?.GetMethod() ?? throw new InvalidOperationException(nameof(AutoStart));
			string className = methodMemberInfo.DeclaringType.Name;
			string methodName = methodMemberInfo.Name;

			return Start(className, methodName, threadId);
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

		private void Dispose(bool disposing)
		{
			if (!_disposed && disposing)
			{
				End();

				if (!MethodsStack.Any())
				{
					if (_isMultiThreaded)
						_threadMethodStacks.TryRemove(Thread.CurrentThread.ManagedThreadId, out _);

					_collector.Dispose();

					_disposed = true;
				}
			}
		}
	}
}