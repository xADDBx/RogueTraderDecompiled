using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
public readonly struct GridDirection
{
	public const ushort kIndexInvalid = ushort.MaxValue;

	public const int kIndexE = 0;

	private const int kIndexNE = 1;

	private const int kIndexN = 2;

	private const int kIndexNW = 3;

	private const int kIndexW = 4;

	private const int kIndexSW = 5;

	private const int kIndexS = 6;

	private const int kIndexSE = 7;

	private const int kIndicesCount = 8;

	public static readonly GridDirection E = new GridDirection(0);

	public static readonly GridDirection NE = new GridDirection(1);

	public static readonly GridDirection N = new GridDirection(2);

	public static readonly GridDirection NW = new GridDirection(3);

	public static readonly GridDirection W = new GridDirection(4);

	public static readonly GridDirection SW = new GridDirection(5);

	public static readonly GridDirection S = new GridDirection(6);

	public static readonly GridDirection SE = new GridDirection(7);

	private const int kTurnCounterClockwise90AdjacencyIndexOffset = 2;

	private const int kTurnClockwise90AdjacencyIndexOffset = 6;

	private const int kTurn180AdjacencyIndexOffset = 4;

	public readonly int m_Index;

	public override string ToString()
	{
		return m_Index switch
		{
			0 => "E", 
			1 => "NE", 
			2 => "N", 
			3 => "NW", 
			4 => "W", 
			5 => "SW", 
			6 => "S", 
			7 => "SE", 
			_ => "Invalid", 
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public GridDirection(int index)
	{
		m_Index = index;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public GridDirection TurnCounterClockwise90()
	{
		return new GridDirection((m_Index + 2) % 8);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public GridDirection TurnClockwise90()
	{
		return new GridDirection((m_Index + 6) % 8);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public GridDirection Turn180()
	{
		return new GridDirection((m_Index + 4) % 8);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static GridDirection operator -(GridDirection value)
	{
		return value.Turn180();
	}
}
