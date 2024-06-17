using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.MemoryManagement;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Constraints;

public class ConstraintSoA : StructureOfArrayBase
{
	public NativeArray<int> Index0;

	public NativeArray<int> Index1;

	public NativeArray<int> Index2;

	public NativeArray<int> Index3;

	public NativeArray<float4> Parameters0;

	public NativeArray<float4> Parameters1;

	public NativeArray<int> Type;

	public NativeArray<int> Id;

	public Constraint this[int index]
	{
		get
		{
			Constraint result = default(Constraint);
			result.id = Id[index];
			result.index0 = Index0[index];
			result.index1 = Index1[index];
			result.index2 = Index2[index];
			result.index3 = Index3[index];
			result.parameters0 = Parameters0[index];
			result.parameters1 = Parameters1[index];
			result.type = (ConstraintType)Type[index];
			return result;
		}
		set
		{
			Id[index] = value.id;
			Index0[index] = value.index0;
			Index1[index] = value.index1;
			Index2[index] = value.index2;
			Index3[index] = value.index3;
			Parameters0[index] = value.parameters0;
			Parameters1[index] = value.parameters1;
			Type[index] = (int)value.type;
		}
	}

	public ConstraintSoA()
		: this(64)
	{
	}

	public ConstraintSoA(int size)
		: base(size)
	{
		m_Allocator.Stride = 40;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		Index0 = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Index1 = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Index2 = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Index3 = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Parameters0 = new NativeArray<float4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Parameters1 = new NativeArray<float4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Type = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Id = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void Dispose()
	{
		if (Index0.IsCreated)
		{
			Index0.Dispose();
			Index1.Dispose();
			Index2.Dispose();
			Index3.Dispose();
			Parameters0.Dispose();
			Parameters1.Dispose();
			Type.Dispose();
			Id.Dispose();
		}
	}
}
