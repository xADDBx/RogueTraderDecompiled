using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.PositionBasedDynamics.Broadphase;

public class GPUSpatialHashmapSoA : GPUSoABase
{
	private static int _SpatialHashmapKeysBuffer = Shader.PropertyToID("_SpatialHashmapKeysBuffer");

	private static int _SpatialHashmapValuesBuffer = Shader.PropertyToID("_SpatialHashmapValuesBuffer");

	public ComputeBufferWrapper<uint> KeysBuffer;

	public ComputeBufferWrapper<uint> ValuesBuffer;

	public override string Name => "GPUSpatialHashmapSoA";

	public GPUSpatialHashmapSoA(int size)
		: base(size)
	{
	}

	protected override ComputeBufferWrapper[] InitBuffers(int size)
	{
		return new ComputeBufferWrapper[2]
		{
			KeysBuffer = new ComputeBufferWrapper<uint>("_SpatialHashmapKeysBuffer", size),
			ValuesBuffer = new ComputeBufferWrapper<uint>("_SpatialHashmapValuesBuffer", size)
		};
	}
}
