using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;

public class GPUForceVolumeSoA : GPUSoABase
{
	public static int _PbdForceVolumeEnumPackedValuesBuffer = Shader.PropertyToID("_PbdForceVolumeEnumPackedValuesBuffer");

	public static int _PbdForceVolumeParametersBuffer = Shader.PropertyToID("_PbdForceVolumeParametersBuffer");

	public static int _PbdForceVolumeEmissionParametersBuffer = Shader.PropertyToID("_PbdForceVolumeEmissionParametersBuffer");

	public ComputeBufferWrapper<int> EnumPackedValuesBuffer;

	public ComputeBufferWrapper<float4x2> VolumeParametersBuffer;

	public ComputeBufferWrapper<float4x3> EmissionParametersBuffer;

	public override string Name => "GPUForceVolumeSoA";

	protected override ComputeBufferWrapper[] InitBuffers(int size)
	{
		return new ComputeBufferWrapper[3]
		{
			EnumPackedValuesBuffer = new ComputeBufferWrapper<int>("_PbdForceVolumeEnumPackedValuesBuffer", size),
			VolumeParametersBuffer = new ComputeBufferWrapper<float4x2>("_PbdForceVolumeParametersBuffer", size),
			EmissionParametersBuffer = new ComputeBufferWrapper<float4x3>("_PbdForceVolumeEmissionParametersBuffer", size)
		};
	}
}
