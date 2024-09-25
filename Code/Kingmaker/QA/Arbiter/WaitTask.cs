using System;
using System.Collections;

namespace Kingmaker.QA.Arbiter;

public class WaitTask : ArbiterTask
{
	private readonly Func<bool> m_Predicate;

	private readonly TimeSpan m_WaitLimit = TimeSpan.FromSeconds(600.0);

	public WaitTask(ArbiterTask parent, Func<bool> predicate)
		: base(parent)
	{
		m_Predicate = predicate;
	}

	public WaitTask(ArbiterTask parent)
		: base(parent)
	{
		m_Predicate = Predicate;
	}

	protected virtual bool Predicate()
	{
		return true;
	}

	protected sealed override IEnumerator Routine()
	{
		yield return new DelayTask(TimeSpan.FromSeconds(1.0), this);
		DateTime startTime = DateTime.Now;
		while (!m_Predicate())
		{
			TimeSpan diff = DateTime.Now - startTime;
			base.Status = $"{base.ParentTask?.Status} - wait for ({diff:g})";
			yield return new DelayTask(TimeSpan.FromSeconds(5.0), this);
			if (diff > m_WaitLimit)
			{
				throw new ArbiterException(base.Status + " - timeout is reached");
			}
		}
	}
}
