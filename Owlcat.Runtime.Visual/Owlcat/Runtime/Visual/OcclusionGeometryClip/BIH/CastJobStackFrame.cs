using Unity.Burst;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip.BIH;

[BurstCompile]
internal struct CastJobStackFrame
{
	public int nodeIndex;

	public ABox nodeBounds;

	public int axis;
}
