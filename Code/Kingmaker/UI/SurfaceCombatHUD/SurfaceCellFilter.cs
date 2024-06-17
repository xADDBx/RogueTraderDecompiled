using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal readonly struct SurfaceCellFilter
{
	private readonly int m_StrictTestMask;

	private readonly int m_OptionalTestMask;

	private readonly int m_StrictTestReference;

	private readonly CellBuffer m_CellBuffer;

	public SurfaceCellFilter(CellBuffer cellBuffer, SurfaceCellFilterData data)
	{
		m_CellBuffer = cellBuffer;
		m_StrictTestMask = data.belongToAllAreaMask | data.notBelongToAnyAreasMask;
		m_StrictTestReference = data.belongToAllAreaMask;
		m_OptionalTestMask = data.belongToAnyAreasMask;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Test(int cellIndex)
	{
		int areaMask = m_CellBuffer.GetAreaMask(cellIndex);
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
}
