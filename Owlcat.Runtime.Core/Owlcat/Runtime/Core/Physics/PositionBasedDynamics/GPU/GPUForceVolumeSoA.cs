using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;

public class GPUForceVolumeSoA : GPUSoABase
{
	public struct ForceVolumeInfo
	{
		private int enumPackedValue;

		private float4x2 parameters;

		private float4x3 emissionParameters;
	}

	public static int _PbdForceVolumeInfoBuffer = Shader.PropertyToID("_PbdForceVolumeInfoBuffer");

	public ComputeBufferWrapper<ForceVolumeInfo> InfoBuffer;

	public override string Name => "GPUForceVolumeSoA";

	protected override ComputeBufferWrapper[] InitBuffers(int size)
	{
		return new ComputeBufferWrapper[1] { InfoBuffer = new ComputeBufferWrapper<ForceVolumeInfo>("_PbdForceVolumeInfoBuffer", size) };
	}
}
