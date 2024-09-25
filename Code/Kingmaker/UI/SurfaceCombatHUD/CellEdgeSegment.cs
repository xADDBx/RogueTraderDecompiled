using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal readonly struct CellEdgeSegment
{
	public const byte kEastSideSouthPart = 0;

	public const byte kEastSideNorthPart = 1;

	public const byte kNorthSideEastPart = 2;

	public const byte kNorthSideWestPart = 3;

	public const byte kWestSideNorthPart = 4;

	public const byte kWestSideSouthPart = 5;

	public const byte kSouthSideWestPart = 6;

	public const byte kSouthSideEastPart = 7;

	private readonly int m_Index;

	public byte Flag
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (byte)(1 << m_Index);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public CellEdgeSegment(int index)
	{
		m_Index = index;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public CellEdgeSegment TurnClockwise90()
	{
		return new CellEdgeSegment((m_Index + 6) % 8);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public CellEdgeSegment TurnCounterClockwise90()
	{
		return new CellEdgeSegment((m_Index + 2) % 8);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public CellEdgeSegment Turn180()
	{
		return new CellEdgeSegment((m_Index + 4) % 8);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public CellEdgeSegment Shift(int value)
	{
		return new CellEdgeSegment((m_Index + 8 + value) % 8);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public CellEdgeSegment MirrorSide()
	{
		int num = m_Index % 2;
		int num2 = m_Index - num + 4;
		int num3 = (num + 1) % 2;
		return new CellEdgeSegment((num2 + num3) % 8);
	}

	[BurstCompile]
	public GridDirection GetAdjacentCellDirection()
	{
		return new GridDirection(m_Index / 2 * 2);
	}

	public override string ToString()
	{
		return m_Index switch
		{
			0 => "{East Side, South Part}", 
			1 => "{East Side, North Part}", 
			2 => "{North Side, East Part}", 
			3 => "{North Side, West Part}", 
			4 => "{West Side, North Part}", 
			5 => "{West Side, South Part}", 
			6 => "{South Side, West Part}", 
			7 => "{South Side, East Part}", 
			_ => "Invalid", 
		};
	}
}
