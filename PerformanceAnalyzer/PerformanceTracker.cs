namespace Skyline.DataMiner.Utils.PerformanceAnalyzer
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.CompilerServices;
	using System.Threading;

	using Skyline.DataMiner.Utils.PerformanceAnalyzer.Models;

	/// <summary>
	/// <see cref="PerformanceTracker"/> tracks method calls in single or multi threaded environments.
	/// </summary>
	public sealed class PerformanceTracker : IDisposable
	{
		private static readonly ConcurrentDictionary<int, Stack<PerformanceData>> PerThreadStack = new ConcurrentDictionary<int, Stack<PerformanceData>>();

		private readonly bool isMultiThreaded;
		private readonly PerformanceCollector collector;
		private readonly int threadId = Thread.CurrentThread.ManagedThreadId;

		private PerformanceData trackedMethod;
		private bool disposed;
		private bool isSubMethod;

		/// <summary>
		/// Initializes a new instance of the <see cref="PerformanceTracker"/> class and starts performance tracking for the method in which it was initialized.
		/// </summary>
		/// <param name="collector"><see cref="PerformanceCollector"/> to use.</param>
		/// <exception cref="ArgumentNullException">Throws if <paramref name="collector"/> is null.</exception>
		public PerformanceTracker(PerformanceCollector collector) : this()
		{
			this.collector = collector ?? throw new ArgumentNullException(nameof(collector));

			Start(threadId);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PerformanceTracker"/> class and starts performance tracking for the method <paramref name="methodName"/> of the class <paramref name="className"/>.
		/// </summary>
		/// <param name="collector"><see cref="PerformanceCollector"/> to use.</param>
		/// <param name="className">Name of the class from which a method is tracked.</param>
		/// <param name="methodName">Name of the method that is tracked.</param>
		/// <exception cref="ArgumentNullException">Throws if <paramref name="collector"/> is null.</exception>
		public PerformanceTracker(PerformanceCollector collector, string className, string methodName) : this()
		{
			if (String.IsNullOrWhiteSpace(className))
			{
				throw new ArgumentNullException(nameof(className));
			}

			if (String.IsNullOrWhiteSpace(methodName))
			{
				throw new ArgumentNullException(nameof(methodName));
			}

			this.collector = collector ?? throw new ArgumentNullException(nameof(collector));

			Start(className, methodName, threadId);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PerformanceTracker"/> class and starts performance tracking for the method in which it was initialized.
		/// </summary>
		/// <param name="parentPerformanceTracker">Parent <see cref="PerformanceTracker"/> of the new instance. This controls the nesting of the <see cref="PerformanceData"/> for methods in multithreaded use cases.</param>
		/// <exception cref="ArgumentNullException">Throws if parent <paramref name="parentPerformanceTracker"/> is null.</exception>
		public PerformanceTracker(PerformanceTracker parentPerformanceTracker) : this()
		{
			collector = parentPerformanceTracker?.Collector ?? throw new ArgumentNullException(nameof(parentPerformanceTracker));

			PerformanceData methodData = Start(parentPerformanceTracker.threadId);
			methodData.Parent = parentPerformanceTracker.trackedMethod;

			if (Thread.CurrentThread.ManagedThreadId != parentPerformanceTracker.threadId && !isSubMethod)
			{
				parentPerformanceTracker.trackedMethod.SubMethods.Add(methodData);
				isSubMethod = true;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PerformanceTracker"/> class and starts performance tracking for the method <paramref name="methodName"/> of the class <paramref name="className"/>.
		/// </summary>
		/// <param name="parentPerformanceTracker">Parent <see cref="PerformanceTracker"/> of the new instance. This controls the nesting of the <see cref="PerformanceData"/> for methods in multithreaded use cases.</param>
		/// <param name="className">Name of the class from which a method is tracked.</param>
		/// <param name="methodName">Name of the method that is tracked.</param>
		/// <exception cref="ArgumentNullException">Throws if parent <paramref name="parentPerformanceTracker"/> is null.</exception>
		public PerformanceTracker(PerformanceTracker parentPerformanceTracker, string className, string methodName) : this()
		{
			if (String.IsNullOrWhiteSpace(className))
			{
				throw new ArgumentNullException(nameof(className));
			}

			if (String.IsNullOrWhiteSpace(methodName))
			{
				throw new ArgumentNullException(nameof(methodName));
			}

			collector = parentPerformanceTracker?.Collector ?? throw new ArgumentNullException(nameof(parentPerformanceTracker));

			PerformanceData methodData = Start(className, methodName, parentPerformanceTracker.threadId);
			methodData.Parent = parentPerformanceTracker.trackedMethod;

			if (Thread.CurrentThread.ManagedThreadId != parentPerformanceTracker.threadId && !isSubMethod)
			{
				parentPerformanceTracker.trackedMethod.SubMethods.Add(methodData);
				isSubMethod = true;
			}
		}

		private PerformanceTracker()
		{
			if (PerThreadStack.TryAdd(threadId, new Stack<PerformanceData>()))
			{
				isMultiThreaded = PerThreadStack.Count > 1;
			}
		}

		/// <summary>
		/// Gets underlying <see cref="PerformanceCollector"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">Throws if collector is not initialized yet.</exception>
		public PerformanceCollector Collector => collector;

		/// <summary>
		/// Gets <see cref="PerformanceData"/> of the tracked method.
		/// </summary>
		/// <exception cref="InvalidOperationException">Throws if tracked method is not initialized yet.</exception>
		public PerformanceData TrackedMethod => trackedMethod;

		/// <summary>
		/// Gets elapsed time since the initialization of the underlying <see cref="PerformanceCollector"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">Throws if collector is not initialized yet.</exception>
		public TimeSpan Elapsed => collector.Clock.UtcNow - trackedMethod.StartTime;

		private Stack<PerformanceData> Stack => PerThreadStack[threadId];

		/// <summary>
		/// Adds metadata for the tracked method.
		/// </summary>
		/// <param name="key">Key of the metadata.</param>
		/// <param name="value">Value of the metadata.</param>
		/// <returns>Returns current instance of <see cref="PerformanceTracker"/>.</returns>
		public PerformanceTracker AddMetadata(string key, string value)
		{
			trackedMethod.Metadata[key] = value;
			return this;
		}

		/// <summary>
		/// Adds metadata for the tracked method.
		/// </summary>
		/// <param name="metadata">Metadata to add or update.</param>
		/// <returns>Returns current instance of <see cref="PerformanceTracker"/>.</returns>
		public PerformanceTracker AddMetadata(IReadOnlyDictionary<string, string> metadata)
		{
			foreach (var data in metadata)
			{
				trackedMethod.Metadata[data.Key] = data.Value;
			}

			return this;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private PerformanceData Start(int parentThreadId)
		{
			MethodBase methodMemberInfo = new StackTrace().GetFrames()?.Where(frame => frame.GetMethod().Name != ".ctor").Skip(1).FirstOrDefault()?.GetMethod() ?? throw new InvalidOperationException("Unable to retrieve the stack information.");
			string className = methodMemberInfo.DeclaringType.Name;
			string methodName = methodMemberInfo.Name;

			return Start(className, methodName, parentThreadId);
		}

		private PerformanceData Start(string className, string methodName, int threadId)
		{
			var methodData = new PerformanceData(className, methodName);

			if (Stack.Any())
			{
				Stack.Peek().SubMethods.Add(methodData);
				methodData.Parent = Stack.Peek();
				isSubMethod = true;
			}

			Stack.Push(collector.Start(methodData, threadId));

			trackedMethod = methodData;

			return methodData;
		}

		private void End()
		{
			if (Stack.Any())
			{
				collector.Stop(Stack.Pop());
			}
		}

		/// <summary>
		/// Completes performance tracking of the method and adds the data to the collector for logging.
		/// </summary>
#pragma warning disable SA1202 // Elements should be ordered by access
		public void Dispose()
#pragma warning restore SA1202 // Elements should be ordered by access
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!disposed && disposing)
			{
				End();

				if (!Stack.Any())
				{
					if (isMultiThreaded)
					{
						PerThreadStack.TryRemove(threadId, out _);
					}

					collector.Dispose();

					disposed = true;
				}
			}
		}
	}
}