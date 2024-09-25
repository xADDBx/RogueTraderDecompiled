using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Squads;

namespace Kingmaker.AI.BehaviourTrees;

public class Blackboard
{
	public Stack<BehaviourTreeNode> Stack = new Stack<BehaviourTreeNode>();

	public MechanicEntity Entity;

	public DecisionContext DecisionContext = new DecisionContext();

	public bool IsFinishedTurn;

	public BaseUnitEntity Unit
	{
		get
		{
			if (!(Entity is UnitSquad squad))
			{
				return Entity as BaseUnitEntity;
			}
			return squad.SelectLeader();
		}
	}

	public void Reset()
	{
		IsFinishedTurn = false;
		Stack.Clear();
		DecisionContext.ReleaseUnit();
	}
}
