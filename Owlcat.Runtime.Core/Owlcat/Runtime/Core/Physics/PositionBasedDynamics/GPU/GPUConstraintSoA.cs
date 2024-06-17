using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;

public class GPUConstraintSoA : GPUSoABase
{
	public static int _PbdConstraintsIndex0Buffer = Shader.PropertyToID("_PbdConstraintsIndex0Buffer");

	public static int _PbdConstraintsIndex1Buffer = Shader.PropertyToID("_PbdConstraintsIndex1Buffer");

	public static int _PbdConstraintsIndex2Buffer = Shader.PropertyToID("_PbdConstraintsIndex2Buffer");

	public static int _PbdConstraintsIndex3Buffer = Shader.PropertyToID("_PbdConstraintsIndex3Buffer");

	public static int _PbdConstraintsParameters0Buffer = Shader.PropertyToID("_PbdConstraintsParameters0Buffer");

	public static int _PbdConstraintsParameters1Buffer = Shader.PropertyToID("_PbdConstraintsParameters1Buffer");

	public static int _PbdConstraintsTypeBuffer = Shader.PropertyToID("_PbdConstraintsTypeBuffer");

	public ComputeBufferWrapper<int> Index0Buffer;

	public ComputeBufferWrapper<int> Index1Buffer;

	public ComputeBufferWrapper<int> Index2Buffer;

	public ComputeBufferWrapper<int> Index3Buffer;

	public ComputeBufferWrapper<float4> Parameters0Buffer;

	public ComputeBufferWrapper<float4> Parameters1Buffer;

	public ComputeBufferWrapper<int> TypeBuffer;

	public override string Name => "GPUConstraintSoA";

	protected override ComputeBufferWrapper[] InitBuffers(int size)
	{
		return new ComputeBufferWrapper[7]
		{
			Index0Buffer = new ComputeBufferWrapper<int>("_PbdConstraintsIndex0Buffer", size),
			Index1Buffer = new ComputeBufferWrapper<int>("_PbdConstraintsIndex1Buffer", size),
			Index2Buffer = new ComputeBufferWrapper<int>("_PbdConstraintsIndex2Buffer", size),
			Index3Buffer = new ComputeBufferWrapper<int>("_PbdConstraintsIndex3Buffer", size),
			Parameters0Buffer = new ComputeBufferWrapper<float4>("_PbdConstraintsParameters0Buffer", size),
			Parameters1Buffer = new ComputeBufferWrapper<float4>("_PbdConstraintsParameters1Buffer", size),
			TypeBuffer = new ComputeBufferWrapper<int>("_PbdConstraintsTypeBuffer", size)
		};
	}
}
