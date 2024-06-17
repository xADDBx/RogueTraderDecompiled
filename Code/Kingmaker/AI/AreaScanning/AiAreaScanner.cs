using System.Collections.Generic;
using System.Threading.Tasks;
using Kingmaker.AI.DebugUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.AI.AreaScanning;

public class AiAreaScanner
{
	public struct PathData
	{
		public Dictionary<GraphNode, WarhammerPathAiCell> cells;

		public WarhammerPathAiCell startCell;

		public static PathData Zero
		{
			get
			{
				PathData result = default(PathData);
				result.cells = null;
				return result;
			}
		}

		public bool IsZero => cells == null;
	}

	public static async Task<PathData> FindAllReachableNodesAsync(BaseUnitEntity unit, Vector3 pos, float maxPathLen)
	{
		if (!unit.State.CanMove)
		{
			AILogger.Instance.Log(new AILogMessage($"Unit {unit} cant move, find nodes skipped"));
			return PathData.Zero;
		}
		CustomGridNodeBase startNode = (CustomGridNodeBase)AstarPath.active.GetNearest(pos).node;
		if (startNode == null)
		{
			return PathData.Zero;
		}
		Dictionary<GraphNode, AiBrainHelper.ThreatsInfo> threateningAreaCells = AiBrainHelper.GatherThreatsData(unit);
		Dictionary<GraphNode, WarhammerPathAiCell> dictionary = await PathfindingService.Instance.FindAllReachableTiles_Delayed_Task(unit.View.MovementAgent, pos, (int)maxPathLen, threateningAreaCells);
		if (dictionary == null)
		{
			AILogger.Instance.Error(new AILogMessage($"WarhammerPath result is null for unit {unit}"));
			return PathData.Zero;
		}
		if (!dictionary.ContainsKey(startNode))
		{
			AILogger.Instance.Error(new AILogMessage($"WarhammerPath result is weird: unit={unit}, startPos={pos}, startNode={startNode}, result.Count={dictionary.Count}"));
			foreach (var (arg, warhammerPathAiCell2) in dictionary)
			{
				AILogger.Instance.Log(new AILogMessage($"Node: {arg}, PathNode: {warhammerPathAiCell2}"));
			}
			return PathData.Zero;
		}
		PathData result = default(PathData);
		result.cells = dictionary;
		result.startCell = dictionary[startNode];
		return result;
	}
}
