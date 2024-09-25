using Kingmaker;
using Kingmaker.AI.BehaviourTrees;
using Kingmaker.AI.BehaviourTrees.Nodes;
using Kingmaker.AreaLogic.TimeSurvival;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands;

namespace Warhammer.SpaceCombat.AI.BehaviourTrees;

public class TaskNodeWaitSpawnTimeSurvival : TaskNode
{
	protected override Status TickInternal(Blackboard blackboard)
	{
		UnitDoNothingParams cmdParams = new UnitDoNothingParams(Game.Instance.CurrentlyLoadedArea.GetComponent<TimeSurvival>()?.StartingBuff?.GetComponent<StarshipSpawnParameters>()?.SpawnWaitDuration ?? 2.5f);
		blackboard.Unit.Commands.Run(cmdParams);
		return Status.Success;
	}
}
