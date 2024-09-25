using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct FillBufferWriter
{
	private readonly CellBuffer m_CellBuffer;

	private FillBuffer m_FillBuffer;

	private MaterialAreaDescriptorBuffer m_MaterialAreaDescriptorBuffer;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public FillBufferWriter(CellBuffer cellBuffer, FillBuffer fillBuffer, MaterialAreaDescriptorBuffer materialAreaDescriptorBuffer)
	{
		m_CellBuffer = cellBuffer;
		m_FillBuffer = fillBuffer;
		m_MaterialAreaDescriptorBuffer = materialAreaDescriptorBuffer;
	}

	public void Write(int materialId, SurfaceCellFilterData filterData)
	{
		SurfaceCellFilter surfaceCellFilter = new SurfaceCellFilter(m_CellBuffer, filterData);
		int2 @int = new int2(int.MaxValue);
		int2 int2 = new int2(int.MinValue);
		for (int i = 0; i < m_CellBuffer.Length; i++)
		{
			if (surfaceCellFilter.Test(i))
			{
				m_FillBuffer[i] = (byte)materialId;
				int2 coords = m_CellBuffer.GetCell(i).coords;
				@int = math.min(@int, coords);
				int2 = math.max(int2, coords);
			}
		}
		MaterialAreaDescriptor descriptor = default(MaterialAreaDescriptor);
		descriptor.materialId = (byte)materialId;
		descriptor.coordsMin = @int;
		descriptor.coordsMax = int2;
		m_MaterialAreaDescriptorBuffer.Add(descriptor);
	}

	public void Write(int materialId, NativeSlice<int> cellIndices, SurfaceCellFilterData filterData)
	{
		SurfaceCellFilter surfaceCellFilter = new SurfaceCellFilter(m_CellBuffer, filterData);
		int2 @int = new int2(int.MaxValue);
		int2 int2 = new int2(int.MinValue);
		for (int i = 0; i < cellIndices.Length; i++)
		{
			int num = cellIndices[i];
			if (surfaceCellFilter.Test(num))
			{
				m_FillBuffer[num] = (byte)materialId;
				int2 coords = m_CellBuffer.GetCell(num).coords;
				@int = math.min(@int, coords);
				int2 = math.max(int2, coords);
			}
		}
		MaterialAreaDescriptor descriptor = default(MaterialAreaDescriptor);
		descriptor.materialId = (byte)materialId;
		descriptor.coordsMin = @int;
		descriptor.coordsMax = int2;
		m_MaterialAreaDescriptorBuffer.Add(descriptor);
	}
}
