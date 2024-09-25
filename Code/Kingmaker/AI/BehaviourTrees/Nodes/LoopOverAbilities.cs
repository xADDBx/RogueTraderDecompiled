using Kingmaker.AI.DebugUtilities;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class LoopOverAbilities : Loop
{
	public LoopOverAbilities(BehaviourTreeNode node, ExitCondition exitCondition = ExitCondition.NoCondition)
		: base(delegate(Blackboard b)
		{
			b.DecisionContext.InitAbilitiesEnumerator();
		}, delegate(Blackboard b)
		{
			DecisionContext decisionContext = b.DecisionContext;
			decisionContext.ConsiderNextAbility();
			if (decisionContext.ConsideringAbility != null)
			{
				AILogger.Instance.Log(AILogAbility.ConsiderAbility(decisionContext.ConsideringAbility));
			}
			return decisionContext.ConsideringAbility != null;
		}, node, exitCondition)
	{
	}
}
