using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.MemoryManagement;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Forces;

public class ForceVolumeUpdateBuffersSoA : StructureOfArrayBase
{
	public NativeArray<int> EnumPackedValues;

	public NativeArray<float4x4> LocalToWorldVolumeMatrices;

	public NativeArray<float4> VolumeParameters0;

	public NativeArray<float4x4> LocalToWorldEmitterMatrices;

	public NativeArray<float3> EmitterDirection;

	public NativeArray<float> EmitterDirectionLerp;

	public NativeArray<float> EmitterIntensity;

	public ForceVolumeUpdateBuffersSoA()
		: base(256)
	{
		m_Allocator.Stride = 168;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		EnumPackedValues = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		LocalToWorldVolumeMatrices = new NativeArray<float4x4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		VolumeParameters0 = new NativeArray<float4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		LocalToWorldEmitterMatrices = new NativeArray<float4x4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		EmitterDirection = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		EmitterDirectionLerp = new NativeArray<float>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		EmitterIntensity = new NativeArray<float>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void Dispose()
	{
		if (EnumPackedValues.IsCreated)
		{
			EnumPackedValues.Dispose();
			LocalToWorldVolumeMatrices.Dispose();
			VolumeParameters0.Dispose();
			LocalToWorldEmitterMatrices.Dispose();
			EmitterDirection.Dispose();
			EmitterDirectionLerp.Dispose();
			EmitterIntensity.Dispose();
		}
	}
}
