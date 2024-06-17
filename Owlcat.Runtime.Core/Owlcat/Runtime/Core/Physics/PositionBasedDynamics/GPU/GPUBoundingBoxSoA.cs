using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;

public class GPUBoundingBoxSoA : GPUSoABase
{
	private int _BoundingBoxMin = Shader.PropertyToID("_PbdBroadphaseAabbMinBuffer");

	private int _BoundingBoxMax = Shader.PropertyToID("_PbdBroadphaseAabbMaxBuffer");

	public ComputeBufferWrapper<float3> AabbMinBuffer;

	public ComputeBufferWrapper<float3> AabbMaxBuffer;

	public override string Name => "GPUBoundingBoxSoA";

	protected override ComputeBufferWrapper[] InitBuffers(int size)
	{
		AabbMinBuffer = new ComputeBufferWrapper<float3>("_PbdBroadphaseAabbMinBuffer", size);
		AabbMaxBuffer = new ComputeBufferWrapper<float3>("_PbdBroadphaseAabbMaxBuffer", size);
		return new ComputeBufferWrapper[2] { AabbMinBuffer, AabbMaxBuffer };
	}
}
