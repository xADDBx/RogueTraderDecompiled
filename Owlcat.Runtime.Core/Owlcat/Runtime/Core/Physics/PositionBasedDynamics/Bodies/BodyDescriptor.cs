using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;

public struct BodyDescriptor
{
	public int2 ParticlesOffsetCount;

	public int2 ConstraintsOffsetCount;

	public int2 ConstraintsGroupsOffsetCount;

	public int SkinnedDataOffset;

	public int SkinnedDataCount;

	public int SkinnedBoneIndicesMapOffset;

	public int SkinnedBoneIndicesMapCount;

	public int IndicesOffset;

	public int IndicesCount;

	public int VerticesOffset;

	public int VerticesCount;

	public int VertexTriangleMapOffset;

	public int VertexTriangleMapCount;

	public int VertexTriangleMapOffsetCountOffset;

	public int VertexTriangleMapOffsetCountCount;

	public int2 LocalCollidersOffsetCount;

	public float2 MaterialParameters;

	public float TeleportDistanceTreshold;
}
