using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;

public class GPUBoundingBoxSoA : GPUSoABase
{
	public struct BoundingBoxData
	{
		private float3 min;

		private float3 max;
	}

	private int _BoundingBox = Shader.PropertyToID("_PbdBroadphaseAabbBuffer");

	public ComputeBufferWrapper<BoundingBoxData> AabbBuffer;

	public override string Name => "GPUBoundingBoxSoA";

	protected override ComputeBufferWrapper[] InitBuffers(int size)
	{
		AabbBuffer = new ComputeBufferWrapper<BoundingBoxData>("_PbdBroadphaseAabbBuffer", size);
		return new ComputeBufferWrapper[1] { AabbBuffer };
	}
}
