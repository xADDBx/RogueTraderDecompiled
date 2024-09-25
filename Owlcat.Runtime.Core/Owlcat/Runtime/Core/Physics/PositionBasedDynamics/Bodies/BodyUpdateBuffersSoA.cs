using System.Runtime.InteropServices;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.MemoryManagement;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;

public class BodyUpdateBuffersSoA : StructureOfArrayBase
{
	public NativeArray<float4x4> BoneUpdateParticleMatrices;

	public NativeArray<float4x4> VertexUpdateParticleMatrices;

	public NativeArray<float4x4> ParticleUpdateVertexMatrices;

	public BodyUpdateBuffersSoA()
		: base(256)
	{
		m_Allocator.Stride = Marshal.SizeOf<float4x4>() * 3;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		BoneUpdateParticleMatrices = new NativeArray<float4x4>(256, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		VertexUpdateParticleMatrices = new NativeArray<float4x4>(256, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ParticleUpdateVertexMatrices = new NativeArray<float4x4>(256, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void Dispose()
	{
		if (BoneUpdateParticleMatrices.IsCreated)
		{
			BoneUpdateParticleMatrices.Dispose();
			VertexUpdateParticleMatrices.Dispose();
			ParticleUpdateVertexMatrices.Dispose();
		}
	}
}
