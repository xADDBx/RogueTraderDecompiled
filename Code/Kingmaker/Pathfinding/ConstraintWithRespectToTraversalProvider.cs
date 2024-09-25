using Pathfinding;

namespace Kingmaker.Pathfinding;

public class ConstraintWithRespectToTraversalProvider : PathNNConstraint
{
	private WarhammerSingleNodeBlocker m_Blocker;

	public ConstraintWithRespectToTraversalProvider(WarhammerSingleNodeBlocker blocker)
	{
		m_Blocker = blocker;
		constrainArea = true;
	}

	public override bool Suitable(GraphNode node)
	{
		if (!base.Suitable(node))
		{
			return false;
		}
		return !WarhammerBlockManager.Instance.NodeContainsAnyExcept(node, m_Blocker);
	}

	public override void Reset()
	{
		base.Reset();
		m_Blocker = null;
	}
}
