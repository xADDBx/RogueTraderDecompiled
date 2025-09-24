using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.MemoryManagement;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Constraints;

public class ConstraintSoA : StructureOfArrayBase
{
	public struct ConstraintData
	{
		public int Index0;

		public int Index1;

		public int Index2;

		public int Index3;

		public float4 Parameters0;

		public float4 Parameters1;

		public int Type;
	}

	public NativeArray<ConstraintData> m_ConstraintData;

	public NativeArray<int> Id;

	public Constraint this[int index]
	{
		get
		{
			Constraint result = default(Constraint);
			result.id = Id[index];
			result.index0 = m_ConstraintData[index].Index0;
			result.index1 = m_ConstraintData[index].Index1;
			result.index2 = m_ConstraintData[index].Index2;
			result.index3 = m_ConstraintData[index].Index3;
			result.parameters0 = m_ConstraintData[index].Parameters0;
			result.parameters1 = m_ConstraintData[index].Parameters1;
			result.type = (ConstraintType)m_ConstraintData[index].Type;
			return result;
		}
		set
		{
			Id[index] = value.id;
			ConstraintData value2 = default(ConstraintData);
			value2.Index0 = value.index0;
			value2.Index1 = value.index1;
			value2.Index2 = value.index2;
			value2.Index3 = value.index3;
			value2.Parameters0 = value.parameters0;
			value2.Parameters1 = value.parameters1;
			value2.Type = (int)value.type;
			m_ConstraintData[index] = value2;
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
		m_ConstraintData = new NativeArray<ConstraintData>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Id = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void Dispose()
	{
		if (m_ConstraintData.IsCreated)
		{
			m_ConstraintData.Dispose();
			Id.Dispose();
		}
	}
}
