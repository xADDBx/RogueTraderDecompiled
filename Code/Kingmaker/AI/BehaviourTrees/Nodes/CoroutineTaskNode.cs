using System.Collections.Generic;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public abstract class CoroutineTaskNode : TaskNode
{
	private Status m_LastStatus;

	private IEnumerator<Status> m_Coroutine;

	protected override void InitInternal()
	{
		base.InitInternal();
		m_LastStatus = Status.Unknown;
		m_Coroutine = null;
	}

	protected override Status TickInternal(Blackboard blackboard)
	{
		if (m_Coroutine == null)
		{
			m_Coroutine = CreateCoroutine(blackboard);
		}
		if (m_Coroutine.MoveNext())
		{
			m_LastStatus = m_Coroutine.Current;
		}
		return m_LastStatus;
	}

	protected abstract IEnumerator<Status> CreateCoroutine(Blackboard blackboard);
}
