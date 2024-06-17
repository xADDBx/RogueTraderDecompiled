using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.MemoryManagement;
using Unity.Collections;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;

public class SpatialHashmapSoA : StructureOfArrayBase
{
	public NativeArray<uint> Keys;

	public NativeArray<uint> Values;

	public NativeArray<uint> FrameId;

	public SpatialHashmapSoA(int size)
		: base(size)
	{
		Resize(size);
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		Keys = new NativeArray<uint>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Values = new NativeArray<uint>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		FrameId = new NativeArray<uint>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void Dispose()
	{
		if (Keys.IsCreated)
		{
			Keys.Dispose();
			Values.Dispose();
			FrameId.Dispose();
		}
	}
}
