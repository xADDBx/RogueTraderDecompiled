using Kingmaker.AI.AreaScanning.Scoring;
using Kingmaker.Pathfinding;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.AI.AreaScanning.TileScorers;

public class LuredTileScorer : TileScorer
{
	protected override Score CalculateClosinessScore(DecisionContext context, CustomGridNodeBase node)
	{
		return new Score(1f / (float)GeometryUtils.GetWarhammerCellDistance(node.Vector3Position, context.LuredTo.Position, GraphParamsMechanicsCache.GridCellSize));
	}
}
