using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.MemoryManagement;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions;

public class ColliderUpdateBuffersSoA : StructureOfArrayBase
{
	public NativeArray<int> Type;

	public NativeArray<float4x4> ObjectToWorld;

	public NativeArray<float4> Parameters0;

	public NativeArray<float4> MaterialParameters;

	public ColliderUpdateBuffersSoA()
		: base(256)
	{
		m_Allocator.Stride = 100;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		Type = new NativeArray<int>(256, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ObjectToWorld = new NativeArray<float4x4>(256, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Parameters0 = new NativeArray<float4>(256, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		MaterialParameters = new NativeArray<float4>(256, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void Dispose()
	{
		if (Type.IsCreated)
		{
			Type.Dispose();
			ObjectToWorld.Dispose();
			Parameters0.Dispose();
			MaterialParameters.Dispose();
		}
	}
}
