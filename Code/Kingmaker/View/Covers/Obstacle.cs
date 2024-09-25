using Kingmaker.Pathfinding;

namespace Kingmaker.View.Covers;

public readonly struct Obstacle
{
	public readonly CustomGridNodeBase Node;

	public readonly int FenceDirection;

	public bool IsFence
	{
		get
		{
			int fenceDirection = FenceDirection;
			if (fenceDirection >= 0)
			{
				return fenceDirection < 8;
			}
			return false;
		}
	}

	public Obstacle(CustomGridNodeBase node, CustomGridNodeBase coveredNode)
	{
		Node = node;
		FenceDirection = ((coveredNode != node && coveredNode.HasFenceWithNode(node)) ? CustomGraphHelper.GuessDirection(coveredNode.Vector3Position - node.Vector3Position) : (-1));
	}

	public Obstacle(CustomGridNodeBase node, int fenceDirection = -1)
	{
		Node = node;
		FenceDirection = fenceDirection;
	}
}
