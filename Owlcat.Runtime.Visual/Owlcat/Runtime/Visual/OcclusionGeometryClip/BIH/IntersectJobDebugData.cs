using Unity.Burst;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip.BIH;

[BurstCompile]
public struct IntersectJobDebugData
{
	public int innerNodeAabbTestCount;

	public int innerNodeGeometryTestCount;

	public int leafNodeTestCount;

	public int maxFrameStackSize;
}
