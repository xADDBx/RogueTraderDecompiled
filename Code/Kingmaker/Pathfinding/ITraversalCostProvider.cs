using Pathfinding;

namespace Kingmaker.Pathfinding;

public interface ITraversalCostProvider<TIntermediateMetric, out TFinalMetric>
{
	int Compare(in TIntermediateMetric lengthA, in GraphNode nodeA, in TIntermediateMetric lengthB, in GraphNode nodeB);

	TIntermediateMetric Calc(in TIntermediateMetric distanceFrom, in GraphNode from, in GraphNode to);

	TFinalMetric Convert(in TIntermediateMetric distance, in GraphNode node, in GraphNode parentNode, in IWarhammerTraversalProvider traversalProvider);

	bool IsWithinRange(in TIntermediateMetric node);

	bool IsTargetNode(in TIntermediateMetric distance, in GraphNode node);
}
