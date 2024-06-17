using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.Burst;
using Unity.Collections;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal readonly struct OutlineCellFilter
{
	[ReadOnly]
	private readonly CellBuffer m_CellBuffer;

	[ReadOnly]
	private readonly FillBuffer m_FillBuffer;

	private readonly SurfaceBufferMask m_SurfaceBufferMask;

	private readonly int m_StrictTestMask;

	private readonly int m_OptionalTestMask;

	private readonly int m_StrictTestReference;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public OutlineCellFilter(CellBuffer cellBuffer, FillBuffer fillBuffer, OutlineCellFilterData data)
	{
		m_CellBuffer = cellBuffer;
		m_FillBuffer = fillBuffer;
		m_SurfaceBufferMask = data.surfaceBuffer;
		m_StrictTestMask = data.belongToAllAreaMask | data.notBelongToAnyAreasMask;
		m_StrictTestReference = data.belongToAllAreaMask;
		m_OptionalTestMask = data.belongToAnyAreasMask;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public bool Test(int cellIndex)
	{
		if (TestArea(m_CellBuffer.GetAreaMask(cellIndex)))
		{
			return TestSurface(cellIndex);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	public bool TestArea(int areaMask)
	{
		if ((areaMask & m_StrictTestMask) != m_StrictTestReference)
		{
			return false;
		}
		if (m_OptionalTestMask != 0 && (areaMask & m_OptionalTestMask) == 0)
		{
			return false;
		}
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Pure]
	private bool TestSurface(int cellIndex)
	{
		return m_SurfaceBufferMask switch
		{
			SurfaceBufferMask.HasValue => m_FillBuffer.HasValue(cellIndex), 
			SurfaceBufferMask.HasNoValue => m_FillBuffer.HasNoValue(cellIndex), 
			_ => true, 
		};
	}
}
