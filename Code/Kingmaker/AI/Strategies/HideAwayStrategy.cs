using Kingmaker.AI.AreaScanning.TileScorers;
using Kingmaker.AI.BehaviourTrees;
using Kingmaker.AI.BehaviourTrees.Nodes;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.AI.Strategies;

public class HideAwayStrategy : AiStrategy
{
	public override BehaviourTreeNode CreateBehaviourTree()
	{
		return new Sequence(new Succeeder(new Sequence(new AsyncTaskNodeCreateMoveVariants(), new TaskNodeFindBetterPlace(new ProtectionTileScorer()), TaskNodeSetupMoveCommand.ToBetterPosition(), new TaskNodeExecuteMoveCommand())), new TaskNodeExecute(delegate(Blackboard b)
		{
			DecisionContext decisionContext = b.DecisionContext;
			AbilityData ability = decisionContext.Ability;
			if ((object)ability != null && ability.Blueprint.Range == AbilityRange.Personal)
			{
				decisionContext.AbilityTarget = decisionContext.Unit;
			}
		}), new TaskNodeCastAbility())
		{
			DebugName = "HideAwayStrategy"
		};
	}
}
