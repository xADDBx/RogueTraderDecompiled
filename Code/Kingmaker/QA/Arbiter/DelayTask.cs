using System;
using System.Collections;

namespace Kingmaker.QA.Arbiter;

public class DelayTask : ArbiterTask
{
	private readonly TimeSpan m_Delay;

	public DelayTask(TimeSpan delay, ArbiterTask parent)
		: base(parent)
	{
		m_Delay = delay;
	}

	protected override IEnumerator Routine()
	{
		DateTime startTime = DateTime.Now;
		TimeSpan timeSpan;
		while ((timeSpan = DateTime.Now - startTime) < m_Delay)
		{
			base.Status = $"{base.ParentTask?.Status} - wait for delay ({timeSpan:g}/{m_Delay:g})";
			yield return null;
		}
	}
}
