using System.Collections.Generic;
using Kingmaker.AI.BehaviourTrees;
using Kingmaker.AI.DebugUtilities;

namespace Kingmaker.AI.Profiling;

public class AIProfileBTreeNodeData : IContextData
{
	public BehaviourTreeNode Node { get; }

	public List<IContextData> Children { get; } = new List<IContextData>();


	public AIProfileContextData Data { get; }

	public AIProfileBTreeNodeData(BehaviourTreeNode node)
	{
		Node = node;
		Data = new AIProfileContextData(Node);
	}

	public void EnterContext()
	{
		Data.EnterContext();
	}

	public void ExitContext()
	{
		Data.ExitContext();
	}
}
