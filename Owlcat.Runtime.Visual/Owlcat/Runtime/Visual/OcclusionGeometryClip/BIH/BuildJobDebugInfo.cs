using Unity.Burst;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip.BIH;

[BurstCompile]
internal struct BuildJobDebugInfo
{
	public int estimatedNodesCount;

	public int nodesCount;

	public int maxFrameStackSize;

	public int minLeafSize;

	public int maxLeafSize;

	public override string ToString()
	{
		return $"{{nodesCount:{nodesCount}, estimatedNodesCount:{estimatedNodesCount}, maxFrameStackSize:{maxFrameStackSize}, minLeafSize:{minLeafSize}, maxLeafSize:{maxLeafSize}}}";
	}
}
