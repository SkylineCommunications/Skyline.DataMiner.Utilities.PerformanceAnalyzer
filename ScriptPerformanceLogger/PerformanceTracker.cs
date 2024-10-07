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

	/// <summary>
	/// <see cref="PerformanceTracker"/> tracks method calls in single or multi threaded environments.
	/// </summary>
	public sealed class PerformanceTracker : IDisposable
	{
		private static readonly ConcurrentDictionary<int, Stack<PerformanceData>> _threadMethodStacks = new ConcurrentDictionary<int, Stack<PerformanceData>>();

		private readonly bool _isMultiThreaded;
		private readonly PerformanceCollector _collector;
		private readonly int _threadId = Thread.CurrentThread.ManagedThreadId;

		private PerformanceData _trackedMethod;
		private bool _disposed;
		private bool _isStarted;

		/// <summary>
		/// Initializes a new instance of the <see cref="PerformanceTracker"/> class.
		/// </summary>
		/// <param name="startNow">True if method tracking should start on initialization; false otherwise.</param>
		public PerformanceTracker(bool startNow = true) : this()
		{
			_collector = new PerformanceCollector(new PerformanceLogger($"default-thread-{Thread.CurrentThread.ManagedThreadId}"));

			if (startNow)
			{
				AutoStart(_threadId);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PerformanceTracker"/> class.
		/// </summary>
		/// <param name="collector"><see cref="PerformanceCollector"/> to use.</param>
		/// <param name="startNow">True if method tracking should start on initialization; false otherwise.</param>
		/// <exception cref="ArgumentNullException">Throws if <paramref name="collector"/> is null.</exception>
		public PerformanceTracker(PerformanceCollector collector, bool startNow = true) : this()
		{
			_collector = collector ?? throw new ArgumentNullException(nameof(collector));

			if (startNow)
			{
				AutoStart(_threadId);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PerformanceTracker"/> class and starts performance tracking for the method in which it was initialized.
		/// </summary>
		/// <param name="parentPerformanceTracker">Parent <see cref="PerformanceTracker"/> of the new instance. This controls the nesting of the <see cref="PerformanceData"/> for methods in multithreaded use cases.</param>
		/// <exception cref="ArgumentNullException">Throws if parent <paramref name="parentPerformanceTracker"/> is null.</exception>
		public PerformanceTracker(PerformanceTracker parentPerformanceTracker) : this()
		{
			_collector = parentPerformanceTracker?.Collector ?? throw new ArgumentNullException(nameof(parentPerformanceTracker));
			if (parentPerformanceTracker._trackedMethod == null)
			{
				throw new InvalidOperationException($"Parent {nameof(PerformanceTracker)} is not started, call Start(string, string).");
			}

			PerformanceData methodData = AutoStart(parentPerformanceTracker._threadId);
			methodData.Parent = parentPerformanceTracker._trackedMethod;

			if (Thread.CurrentThread.ManagedThreadId != parentPerformanceTracker._threadId)
			{
				parentPerformanceTracker._trackedMethod.SubMethods.Add(methodData);
			}
		}

		private PerformanceTracker()
		{
			if (_threadMethodStacks.TryAdd(Thread.CurrentThread.ManagedThreadId, new Stack<PerformanceData>()))
			{
				_isMultiThreaded = _threadMethodStacks.Count > 1;
			}
		}

		/// <summary>
		/// Gets underlying <see cref="PerformanceCollector"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">Throws if collector is not initialized yet.</exception>
		public PerformanceCollector Collector => _collector;

		/// <summary>
		/// Gets <see cref="PerformanceData"/> of the tracked method.
		/// </summary>
		/// <exception cref="InvalidOperationException">Throws if root method is not initialized yet.</exception>
		public PerformanceData TrackedMethod => _trackedMethod ?? throw new InvalidOperationException(nameof(_trackedMethod));

		/// <summary>
		/// Gets elapsed time since the initialization of the underlying <see cref="PerformanceCollector"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">Throws if collector is not initialized yet.</exception>
		public TimeSpan Elapsed
		{
			get
			{
				if (_trackedMethod == null)
				{
					throw new InvalidOperationException("Performance tracking not started, call Start(string, string)");
				}

				return _collector.Clock.UtcNow - _trackedMethod.StartTime;
			}
		}

		private Stack<PerformanceData> MethodsStack => _threadMethodStacks[_threadId];

		/// <summary>
		/// Adds metadata for the tracked method.
		/// </summary>
		/// <param name="key">Key of the metadata.</param>
		/// <param name="value">Value of the metadata.</param>
		/// <returns>Returns current instance of <see cref="PerformanceTracker"/>.</returns>
		public PerformanceTracker AddMetadata(string key, string value)
		{
			_trackedMethod.Metadata[key] = value;
			return this;
		}

		/// <summary>
		/// Creates new instance of <see cref="PerformanceData"/> with specified class and method names and starts performance tracking for it.
		/// </summary>
		/// <param name="className">Name of the class which will be used in new instance of <see cref="PerformanceData"/>.</param>
		/// <param name="methodName">Name of the method which will be used in new instance of <see cref="PerformanceData"/>.</param>
		/// <returns>New instance of <see cref="PerformanceData"/> for the tracked method.</returns>
		public PerformanceData Start(string className, string methodName)
		{
			return Start(className, methodName, _threadId);
		}

		private PerformanceData Start(string className, string methodName, int threadId)
		{
			if (_isStarted)
			{
				return MethodsStack.Peek();
			}

			var methodData = new PerformanceData(className, methodName);

			if (MethodsStack.Any())
			{
				MethodsStack.Peek().SubMethods.Add(methodData);
				methodData.Parent = MethodsStack.Peek();
			}

			MethodsStack.Push(_collector.Start(methodData, threadId));

			_trackedMethod = methodData;
			_isStarted = true;

			return methodData;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private PerformanceData AutoStart(int parentThreadId)
		{
			MethodBase methodMemberInfo = new StackTrace().GetFrames()?.Where(frame => frame.GetMethod().Name != ".ctor").Skip(1).FirstOrDefault()?.GetMethod() ?? throw new InvalidOperationException(nameof(AutoStart));
			string className = methodMemberInfo.DeclaringType.Name;
			string methodName = methodMemberInfo.Name;

			return Start(className, methodName, parentThreadId);
		}

		private void End()
		{
			if (MethodsStack.Any())
			{
				_collector.Stop(MethodsStack.Pop());
			}
		}

		/// <summary>
		/// Completes performance tracking of the method and adds the data to the collector for logging.
		/// </summary>
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
					{
						_threadMethodStacks.TryRemove(Thread.CurrentThread.ManagedThreadId, out _);
					}

					_collector.Dispose();

					_disposed = true;
				}
			}
		}
	}
}