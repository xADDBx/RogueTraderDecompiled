using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.MemoryManagement;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions;

public class ColliderSoA : StructureOfArrayBase
{
	public NativeArray<float4> Parameters0;

	public NativeArray<float4> Parameters1;

	public NativeArray<float4> Parameters2;

	public NativeArray<int> Type;

	public NativeArray<float2> MaterialParameters;

	public ColliderSoA()
		: this(64)
	{
	}

	public ColliderSoA(int size)
		: base(size)
	{
		m_Allocator.Stride = 84;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		Parameters0 = new NativeArray<float4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Parameters1 = new NativeArray<float4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Parameters2 = new NativeArray<float4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Type = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		MaterialParameters = new NativeArray<float2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void Dispose()
	{
		if (Parameters0.IsCreated)
		{
			Parameters0.Dispose();
			Parameters1.Dispose();
			Parameters2.Dispose();
			Type.Dispose();
			MaterialParameters.Dispose();
		}
	}
}
