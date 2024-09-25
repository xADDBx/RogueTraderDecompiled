using System;
using System.Collections;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Kingmaker.QA.Arbiter;

public abstract class ArbiterTask
{
	private Stopwatch m_Stopwatch;

	public string Status { get; protected set; }

	[CanBeNull]
	public ArbiterTask ParentTask { get; }

	public IEnumerator Ticker { get; }

	public TimeSpan ElapsedTestTime => m_Stopwatch.Elapsed;

	public ArbiterTask SubtaskOrSelf => (Ticker?.Current as ArbiterTask) ?? this;

	public float Delay => (Ticker?.Current as float?).GetValueOrDefault();

	protected abstract IEnumerator Routine();

	protected ArbiterTask()
	{
		Ticker = Routine();
		m_Stopwatch = Stopwatch.StartNew();
	}

	protected ArbiterTask(ArbiterTask parent)
	{
		ParentTask = parent;
		Ticker = Routine();
		m_Stopwatch = Stopwatch.StartNew();
	}
}
