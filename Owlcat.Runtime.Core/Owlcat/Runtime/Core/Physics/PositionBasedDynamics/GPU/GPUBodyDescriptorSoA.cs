using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;

public class GPUBodyDescriptorSoA : GPUSoABase
{
	public ComputeBufferWrapper<int2> ParticlesOffsetCountBuffer;

	public ComputeBufferWrapper<int2> ConstraintsOffsetCountBuffer;

	public ComputeBufferWrapper<int2> ConstraintsGroupsOffsetCountBuffer;

	public ComputeBufferWrapper<int> SkinnedDataOffsetBuffer;

	public ComputeBufferWrapper<int> SkinnedDataCountBuffer;

	public ComputeBufferWrapper<int> IndicesOffsetBuffer;

	public ComputeBufferWrapper<int> IndicesCountBuffer;

	public ComputeBufferWrapper<int> VerticesOffsetBuffer;

	public ComputeBufferWrapper<int> VerticesCountBuffer;

	public ComputeBufferWrapper<int> VertexTriangleMapOffsetBuffer;

	public ComputeBufferWrapper<int> VertexTriangleMapCountBuffer;

	public ComputeBufferWrapper<int> VertexTriangleMapOffsetCountOffsetBuffer;

	public ComputeBufferWrapper<int> VertexTriangleMapOffsetCountCountBuffer;

	public ComputeBufferWrapper<int2> LocalCollidersOffsetCountBuffer;

	public ComputeBufferWrapper<float2> MaterialParametersBuffer;

	public ComputeBufferWrapper<float> TeleportDistanceTresholdBuffer;

	public override string Name => "GPUBodyDescriptorSoA";

	protected override ComputeBufferWrapper[] InitBuffers(int size)
	{
		return new ComputeBufferWrapper[16]
		{
			ParticlesOffsetCountBuffer = new ComputeBufferWrapper<int2>("_PbdBodyParticlesOffsetCountBuffer", size),
			ConstraintsOffsetCountBuffer = new ComputeBufferWrapper<int2>("_PbdBodyConstraintsOffsetCountBuffer", size),
			ConstraintsGroupsOffsetCountBuffer = new ComputeBufferWrapper<int2>("_PbdBodyConstraintsGroupsOffsetCountBuffer", size),
			SkinnedDataOffsetBuffer = new ComputeBufferWrapper<int>("_PbdBodySkinnedDataOffsetBuffer", size),
			SkinnedDataCountBuffer = new ComputeBufferWrapper<int>("_PbdBodySkinnedDataCountBuffer", size),
			IndicesOffsetBuffer = new ComputeBufferWrapper<int>("_PbdBodyIndicesOffsetBuffer", size),
			IndicesCountBuffer = new ComputeBufferWrapper<int>("_PbdBodyIndicesCountBuffer", size),
			VerticesOffsetBuffer = new ComputeBufferWrapper<int>("_PbdBodyVerticesOffsetBuffer", size),
			VerticesCountBuffer = new ComputeBufferWrapper<int>("_PbdBodyVerticesCountBuffer", size),
			VertexTriangleMapOffsetBuffer = new ComputeBufferWrapper<int>("_PbdBodyVertexTriangleMapOffsetBuffer", size),
			VertexTriangleMapCountBuffer = new ComputeBufferWrapper<int>("_PbdBodyVertexTriangleMapCountBuffer", size),
			VertexTriangleMapOffsetCountOffsetBuffer = new ComputeBufferWrapper<int>("_PbdBodyVertexTriangleMapOffsetCountOffsetBuffer", size),
			VertexTriangleMapOffsetCountCountBuffer = new ComputeBufferWrapper<int>("_PbdBodyVertexTriangleMapOffsetCountCountBuffer", size),
			LocalCollidersOffsetCountBuffer = new ComputeBufferWrapper<int2>("_PbdBodyLocalCollidersOffsetCountBuffer", size),
			MaterialParametersBuffer = new ComputeBufferWrapper<float2>("_PbdBodyMaterialParametersBuffer", size),
			TeleportDistanceTresholdBuffer = new ComputeBufferWrapper<float>("_PbdBodyTeleportDistanceTreshold", size)
		};
	}
}
