using Kingmaker.AI.BehaviourTrees;

namespace Kingmaker.AI.Strategies;

public abstract class AiStrategy
{
	public abstract BehaviourTreeNode CreateBehaviourTree();
}
