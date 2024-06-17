using System.Collections.Generic;
using Kingmaker.AI.BehaviourTrees;

namespace Kingmaker.AI.Profiling;

public class AIProfileBTreeNodeData
{
	public BehaviourTreeNode Node;

	public AIProfileContextData Data;

	public List<AIProfileBTreeNodeData> Children = new List<AIProfileBTreeNodeData>();

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
