using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.PositionBasedDynamics.Broadphase;

public class GPUSpatialHashmapSoA : GPUSoABase
{
	public struct SpatialHashmapEntry
	{
		private uint key;

		private uint value;
	}

	private static int _SpatialHashmapBuffer = Shader.PropertyToID("_SpatialHashmapBuffer");

	public ComputeBufferWrapper<SpatialHashmapEntry> HashmapBuffer;

	public override string Name => "GPUSpatialHashmapSoA";

	public GPUSpatialHashmapSoA(int size)
		: base(size)
	{
	}

	protected override ComputeBufferWrapper[] InitBuffers(int size)
	{
		return new ComputeBufferWrapper[1] { HashmapBuffer = new ComputeBufferWrapper<SpatialHashmapEntry>("_SpatialHashmapBuffer", size) };
	}
}
