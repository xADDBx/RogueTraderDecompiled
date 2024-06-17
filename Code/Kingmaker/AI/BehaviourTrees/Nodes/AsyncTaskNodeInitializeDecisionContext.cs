using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kingmaker.AI.AreaScanning;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Pathfinding;

namespace Kingmaker.AI.BehaviourTrees.Nodes;

public class AsyncTaskNodeInitializeDecisionContext : AsyncTaskNode
{
	protected override async Task<Status> Process(Blackboard blackboard)
	{
		AILogger.Instance.Log(AILogNode.Start(this));
		DecisionContext decisionContext = blackboard.DecisionContext;
		decisionContext.ReleaseUnit();
		decisionContext.InitCurrentTurnEntity();
		await Task.WhenAll(decisionContext.Enemies.Select((TargetInfo i) => AsyncUpdateEnemyMoveVariants(i)).ToArray());
		AILogger.Instance.Log(AILogNode.End(this));
		return Status.Success;
	}

	private async Task AsyncUpdateEnemyMoveVariants(TargetInfo enemy)
	{
		if (!(enemy.Entity is BaseUnitEntity unit))
		{
			return;
		}
		AiAreaScanner.PathData pathData = await AiAreaScanner.FindAllReachableNodesAsync(unit, enemy.Entity.Position, 2f);
		List<GraphNode> list = new List<GraphNode>();
		int num = 0;
		if (pathData.cells != null)
		{
			foreach (WarhammerPathAiCell value in pathData.cells.Values)
			{
				if ((value.IsCanStand || value.Length == 0f) && (value.Length < 2f || num++ % 2 == 0))
				{
					list.Add(value.Node);
				}
			}
		}
		enemy.AiConsideredMoveVariants = list;
	}
}
