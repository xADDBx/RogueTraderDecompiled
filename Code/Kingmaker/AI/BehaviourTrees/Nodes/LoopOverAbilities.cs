using System;
using Kingmaker.AI.DebugUtilities;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class LoopOverAbilities : Loop
{
	private static readonly Action<Blackboard> Initializer = delegate(Blackboard b)
	{
		b.DecisionContext.InitAbilitiesEnumerator();
	};

	private static readonly Func<Blackboard, bool> MoveNextDelegate = delegate(Blackboard b)
	{
		DecisionContext decisionContext = b.DecisionContext;
		decisionContext.ConsiderNextAbility();
		if (decisionContext.ConsideringAbility != null)
		{
			AILogger.Instance.Log(AILogAbility.ConsiderAbility(decisionContext.ConsideringAbility));
		}
		return decisionContext.ConsideringAbility != null;
	};

	public LoopOverAbilities(BehaviourTreeNode node, ExitCondition exitCondition = ExitCondition.NoCondition)
		: base(Initializer, MoveNextDelegate, "LoopOverAbilities", node, exitCondition)
	{
	}

	public LoopOverAbilities(string debugDescription, BehaviourTreeNode node, ExitCondition exitCondition = ExitCondition.NoCondition)
		: base(debugDescription, Initializer, MoveNextDelegate, "LoopOverAbilities", node, exitCondition)
	{
	}
}
