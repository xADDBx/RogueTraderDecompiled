using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.MemoryManagement;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;

public class BoundingBoxSoA : StructureOfArrayBase
{
	public NativeArray<float3> AabbMin;

	public NativeArray<float3> AabbMax;

	public BoundingBoxSoA()
		: this(1)
	{
	}

	public BoundingBoxSoA(int size)
		: base(size)
	{
		m_Allocator.Stride = 24;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		AabbMin = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		AabbMax = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void Dispose()
	{
		if (AabbMin.IsCreated)
		{
			AabbMin.Dispose();
			AabbMax.Dispose();
		}
	}
}
