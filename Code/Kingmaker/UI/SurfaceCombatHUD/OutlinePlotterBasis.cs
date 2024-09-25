using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct OutlinePlotterBasis
{
	private float3 m_Forward;

	private CellEdgeSegment m_EastSideSouthPartEdge;

	private CornerDirection m_CornerNE;

	private const int kEastSideNorthPartShift = 1;

	private const int kNorthSideEastPartShift = 2;

	private const int kSouthSideWestPartShift = 6;

	private const int kWestSideNorthPartShift = 4;

	public static OutlinePlotterBasis Default
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			OutlinePlotterBasis result = default(OutlinePlotterBasis);
			result.m_Forward = new float3(0f, 0f, 1f);
			result.m_EastSideSouthPartEdge = new CellEdgeSegment(0);
			result.m_CornerNE = new CornerDirection(0);
			return result;
		}
	}

	public float3 Forward
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Forward;
		}
	}

	public float3 Backward
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return -m_Forward;
		}
	}

	public float3 Right
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Forward.zyx * new float3(1f, 1f, -1f);
		}
	}

	public float3 Left
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Forward.zyx * new float3(-1f, 1f, 1f);
		}
	}

	public CellEdgeSegment EastSideNorthPartEdge
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_EastSideSouthPartEdge.Shift(1);
		}
	}

	public CellEdgeSegment EastSideSouthPartEdge
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_EastSideSouthPartEdge;
		}
	}

	public CellEdgeSegment NorthSideEastPartEdge
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_EastSideSouthPartEdge.Shift(2);
		}
	}

	public CellEdgeSegment SouthSideWestPartEdge
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_EastSideSouthPartEdge.Shift(6);
		}
	}

	public CellEdgeSegment WestSideNorthPartEdge
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_EastSideSouthPartEdge.Shift(4);
		}
	}

	public CornerDirection CornerNE
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_CornerNE;
		}
	}

	public CornerDirection CornerNW
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_CornerNE.TurnCounterClockwise90();
		}
	}

	public CornerDirection CornerSW
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_CornerNE.Turn180();
		}
	}

	public CornerDirection CornerSE
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_CornerNE.TurnClockwise90();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TurnCounterClockwise90()
	{
		m_Forward = m_Forward.zyx * new float3(-1f, 0f, 1f);
		m_EastSideSouthPartEdge = m_EastSideSouthPartEdge.TurnCounterClockwise90();
		m_CornerNE = m_CornerNE.TurnCounterClockwise90();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TurnClockwise90()
	{
		m_Forward = m_Forward.zyx * new float3(1f, 0f, -1f);
		m_EastSideSouthPartEdge = m_EastSideSouthPartEdge.TurnClockwise90();
		m_CornerNE = m_CornerNE.TurnClockwise90();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Turn180()
	{
		m_Forward = -m_Forward;
		m_EastSideSouthPartEdge = m_EastSideSouthPartEdge.Turn180();
		m_CornerNE = m_CornerNE.Turn180();
	}
}
