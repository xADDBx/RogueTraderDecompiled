using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.MemoryManagement;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Forces;

public class ForceVolumeSoA : StructureOfArrayBase
{
	public NativeArray<int> EnumPackedValues;

	public NativeArray<float4x2> VolumeParameters;

	public NativeArray<float4x3> EmissionParameters;

	public ForceVolumeSoA(int size)
		: base(size)
	{
		m_Allocator.Stride = 108;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		EnumPackedValues = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		VolumeParameters = new NativeArray<float4x2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		EmissionParameters = new NativeArray<float4x3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void Dispose()
	{
		if (EnumPackedValues.IsCreated)
		{
			EnumPackedValues.Dispose();
			VolumeParameters.Dispose();
			EmissionParameters.Dispose();
		}
	}
}
