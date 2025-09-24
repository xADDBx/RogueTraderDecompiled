using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;

public class GPUColliderSoA : GPUSoABase
{
	public struct ColliderInfo
	{
		private float4 parameters0;

		private float4 parameters1;

		private float4 parameters2;

		private float2 materialParameters;

		private int type;
	}

	public static int _PbdCollidersDataBuffer = Shader.PropertyToID("_PbdCollidersDataBuffer");

	public ComputeBufferWrapper<ColliderInfo> InfoBuffer;

	public override string Name => "GPUColliderSoA";

	protected override ComputeBufferWrapper[] InitBuffers(int size)
	{
		return new ComputeBufferWrapper[1] { InfoBuffer = new ComputeBufferWrapper<ColliderInfo>("_PbdCollidersDataBuffer", size) };
	}
}
