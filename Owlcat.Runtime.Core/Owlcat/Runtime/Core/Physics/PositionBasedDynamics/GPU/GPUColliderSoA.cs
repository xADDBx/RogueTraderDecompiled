using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;

public class GPUColliderSoA : GPUSoABase
{
	public static int _PbdCollidersParameters0Buffer = Shader.PropertyToID("_PbdCollidersParameters0Buffer");

	public static int _PbdCollidersParameters1Buffer = Shader.PropertyToID("_PbdCollidersParameters1Buffer");

	public static int _PbdCollidersParameters2Buffer = Shader.PropertyToID("_PbdCollidersParameters2Buffer");

	public static int _PbdCollidersMaterialParametersBuffer = Shader.PropertyToID("_PbdCollidersMaterialParametersBuffer");

	public static int _PbdCollidersTypeBuffer = Shader.PropertyToID("_PbdCollidersTypeBuffer");

	public ComputeBufferWrapper<float4> Parameters0Buffer;

	public ComputeBufferWrapper<float4> Parameters1Buffer;

	public ComputeBufferWrapper<float4> Parameters2Buffer;

	public ComputeBufferWrapper<float2> MaterialParametersBuffer;

	public ComputeBufferWrapper<int> TypeBuffer;

	public override string Name => "GPUColliderSoA";

	protected override ComputeBufferWrapper[] InitBuffers(int size)
	{
		return new ComputeBufferWrapper[5]
		{
			Parameters0Buffer = new ComputeBufferWrapper<float4>("_PbdCollidersParameters0Buffer", size),
			Parameters1Buffer = new ComputeBufferWrapper<float4>("_PbdCollidersParameters1Buffer", size),
			Parameters2Buffer = new ComputeBufferWrapper<float4>("_PbdCollidersParameters2Buffer", size),
			MaterialParametersBuffer = new ComputeBufferWrapper<float2>("_PbdCollidersMaterialParametersBuffer", size),
			TypeBuffer = new ComputeBufferWrapper<int>("_PbdCollidersTypeBuffer", size)
		};
	}
}
